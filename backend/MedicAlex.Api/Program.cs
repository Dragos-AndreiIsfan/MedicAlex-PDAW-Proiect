using System.Text;
using DotNetEnv;
using MedicAlex.Api.Data;
using MedicAlex.Api.Middleware;
using MedicAlex.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ── Încarcă variabilele din .env ───────────────────────────
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ── Services ───────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Returnează JSON cu camelCase (firstName, lastName, role)
        // în loc de PascalCase (FirstName, LastName, Role)
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

// Swagger cu suport JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "MedicAlex API",
        Version     = "v1",
        Description = "Backend pentru sistemul de gestionare pacienți MedicAlex"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Introdu: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── Database ───────────────────────────────────────────────
builder.Services.AddSingleton<DatabaseContext>();

// ── Application Services ───────────────────────────────────
builder.Services.AddScoped<IJwtService,       JwtService>();
builder.Services.AddScoped<IAuthService,      AuthService>();
builder.Services.AddScoped<IAdminService,     AdminService>();
builder.Services.AddScoped<IMedicinaService,  MedicinaService>();
builder.Services.AddScoped<ILaboratorService, LaboratorService>();

// ── JWT Authentication ─────────────────────────────────────
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET lipsește din fișierul .env");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew                = TimeSpan.Zero  // fără toleranță la expirare
        };

        // Mesaje de eroare clare în română
        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"message\":\"Autentificare necesară.\"}");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"message\":\"Acces interzis.\"}");
            }
        };
    });

builder.Services.AddAuthorization();

// ── CORS — permite Vue dev server ─────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("MedicAlexCors", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",   // Vue dev server
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ══════════════════════════════════════════════════════════
var app = builder.Build();
// ══════════════════════════════════════════════════════════

// ── Middleware pipeline ────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MedicAlex API v1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseCors("MedicAlexCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Verificare conexiune DB + Seed admin ───────────────────
var db = app.Services.GetRequiredService<DatabaseContext>();
try
{
    await db.TestConnectionAsync();
    await db.SeedAdminAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Eroare conectare DB: {ex.Message}");
    Console.WriteLine("   Verifică fișierul .env — DB_HOST, DB_PORT, DB_USER, DB_PASSWORD, DB_NAME");
}

Console.WriteLine("🏥 MedicAlex API pornit pe http://localhost:5000");
Console.WriteLine("📖 Swagger: http://localhost:5000/swagger");

app.Run();
