using Dapper;
using Npgsql;

namespace MedicAlex.Api.Data;

public class DatabaseContext
{
    private readonly string _connectionString;

    public DatabaseContext(IConfiguration config)
    {
        var host     = Environment.GetEnvironmentVariable("DB_HOST")     ?? "localhost";
        var port     = Environment.GetEnvironmentVariable("DB_PORT")     ?? "5432";
        var dbName   = Environment.GetEnvironmentVariable("DB_NAME")     ?? "medicalex";
        var user     = Environment.GetEnvironmentVariable("DB_USER")     ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

        _connectionString = $"Host={host};Port={port};Database={dbName};Username={user};Password={password};";

        // ── Fix critic: Dapper mapează automat snake_case → PascalCase
        // password_hash → PasswordHash, first_name → FirstName, etc.
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task SeedAdminAsync()
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();

        const string checkSql = "SELECT COUNT(*) FROM users WHERE role = 'admin'";
        var count = await conn.ExecuteScalarAsync<int>(checkSql);
        if (count > 0)
        {
            Console.WriteLine("ℹ️  Admin există deja în baza de date.");
            return;
        }

        var hash = BCrypt.Net.BCrypt.HashPassword("Admin@1234", workFactor: 12);

        const string insertSql = """
            INSERT INTO users (email, password_hash, first_name, last_name, role)
            VALUES (@Email, @Hash, 'Administrator', 'MedicAlex', 'admin')
            ON CONFLICT (email) DO UPDATE SET password_hash = @Hash
            """;

        await conn.ExecuteAsync(insertSql, new { Email = "admin@medicalex.ro", Hash = hash });

        Console.WriteLine("✅ Admin creat: admin@medicalex.ro / Admin@1234");
        Console.WriteLine("⚠️  Schimbă parola adminului după primul login!");
    }

    public async Task TestConnectionAsync()
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        var version = await conn.ExecuteScalarAsync<string>("SELECT version()");
        Console.WriteLine($"✅ PostgreSQL conectat: {version?[..version.IndexOf(',')]}");
    }
}
