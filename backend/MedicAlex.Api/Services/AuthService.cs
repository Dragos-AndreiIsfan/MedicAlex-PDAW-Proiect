using Dapper;
using MedicAlex.Api.Data;
using MedicAlex.Api.DTOs;
using MedicAlex.Api.Models;

namespace MedicAlex.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UserDto?> GetProfileAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly DatabaseContext _db;
    private readonly IJwtService _jwt;

    public AuthService(DatabaseContext db, IJwtService jwt)
    {
        _db  = db;
        _jwt = jwt;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        using var conn = _db.CreateConnection();

        const string sql = """
            SELECT id, email, password_hash, first_name, last_name,
                   role, specialization, is_active, created_at, updated_at
            FROM users
            WHERE email = @Email AND is_active = TRUE
            """;

        var user = await conn.QuerySingleOrDefaultAsync<User>(sql, new { request.Email });

        // Email negăsit sau cont inactiv
        if (user is null) return null;

        // Hash invalid (placeholder din seed) — contul nu e încă inițializat
        if (string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2"))
            return null;

        // Parola greșită
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return null;

        var token = _jwt.GenerateToken(user);

        return new LoginResponse(
            Token: token,
            User:  MapToDto(user)
        );
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(
        int userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return (false, "Parola nouă și confirmarea nu coincid.");

        if (request.NewPassword.Length < 6)
            return (false, "Parola trebuie să aibă minim 6 caractere.");

        using var conn = _db.CreateConnection();

        const string getSql = "SELECT password_hash FROM users WHERE id = @UserId AND is_active = TRUE";
        var currentHash = await conn.QuerySingleOrDefaultAsync<string>(getSql, new { UserId = userId });

        if (currentHash is null)
            return (false, "Utilizatorul nu a fost găsit.");

        // Guard: hash invalid
        if (string.IsNullOrEmpty(currentHash) || !currentHash.StartsWith("$2"))
            return (false, "Contul nu are o parolă validă setată. Contactați administratorul.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, currentHash))
            return (false, "Parola curentă este incorectă.");

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);

        const string updateSql = """
            UPDATE users
            SET password_hash = @Hash, updated_at = NOW()
            WHERE id = @UserId
            """;

        await conn.ExecuteAsync(updateSql, new { Hash = newHash, UserId = userId });

        return (true, "Parola a fost schimbată cu succes.");
    }

    public async Task<UserDto?> GetProfileAsync(int userId)
    {
        using var conn = _db.CreateConnection();

        const string sql = """
            SELECT id, email, first_name, last_name, role, specialization
            FROM users
            WHERE id = @UserId AND is_active = TRUE
            """;

        var user = await conn.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });
        return user is null ? null : MapToDto(user);
    }

    private static UserDto MapToDto(User u) => new(
        u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.Specialization
    );
}