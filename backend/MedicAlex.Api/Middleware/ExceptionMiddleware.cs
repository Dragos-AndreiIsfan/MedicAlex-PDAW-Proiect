using System.Net;
using System.Text.Json;

namespace MedicAlex.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare neașteptată: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Forbidden,      "Acces interzis."),
            ArgumentException           => (HttpStatusCode.BadRequest,     ex.Message),
            InvalidOperationException   => (HttpStatusCode.Conflict,       ex.Message),
            _                           => (HttpStatusCode.InternalServerError, "A apărut o eroare internă. Încercați din nou.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { message });
        await context.Response.WriteAsync(response);
    }
}
