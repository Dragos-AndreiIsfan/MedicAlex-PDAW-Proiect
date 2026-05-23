using Dapper;
using MedicAlex.Api.Data;
using MedicAlex.Api.DTOs;
using MedicAlex.Api.Models;

namespace MedicAlex.Api.Services;

public interface IAdminService
{
    Task<IEnumerable<User>>    GetAllDoctorsAsync();
    Task<CreateDoctorResponse> CreateDoctorAsync(CreateDoctorRequest request);
    Task<(bool Success, string Message)> DeleteDoctorAsync(int doctorId);
    Task<IEnumerable<Patient>> GetAllPatientsAsync();
    Task<IdResponse>           CreatePatientAsync(CreatePatientRequest request);
    Task<(bool Success, string Message)> UpdatePatientAsync(int patientId, UpdatePatientRequest request);
    Task<(bool Success, string Message)> DeletePatientAsync(int patientId);
}

public class AdminService : IAdminService
{
    private readonly DatabaseContext _db;

    public AdminService(DatabaseContext db) => _db = db;

    // ── Doctori ────────────────────────────────────────────

    public async Task<IEnumerable<User>> GetAllDoctorsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            SELECT id, email, first_name, last_name, role, specialization, is_active, created_at
            FROM users
            WHERE role != 'admin'
            ORDER BY last_name, first_name
            """;
        return await conn.QueryAsync<User>(sql);
    }

    public async Task<CreateDoctorResponse> CreateDoctorAsync(CreateDoctorRequest request)
    {
        if (request.Role != "doctor_medicina" && request.Role != "doctor_laborator")
            throw new ArgumentException("Rol invalid. Acceptat: doctor_medicina, doctor_laborator");

        // Generează email: prenume.nume@medicalex.ro
        var firstName = RemoveDiacritics(request.FirstName.Trim().ToLower()).Replace(" ", ".");
        var lastName  = RemoveDiacritics(request.LastName.Trim().ToLower()).Replace(" ", ".");
        var email     = $"{firstName}.{lastName}@medicalex.ro";

        // Parola implicită — hash BCrypt cost 12
        const string defaultPassword = "Medic@1234";
        var hash = BCrypt.Net.BCrypt.HashPassword(defaultPassword, workFactor: 12);

        using var conn = _db.CreateConnection();

        // Verifică dacă email-ul există deja
        var exists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM users WHERE email = @Email", new { Email = email });
        if (exists > 0)
            throw new InvalidOperationException($"Email deja existent: {email}");

        const string sql = """
            INSERT INTO users (email, password_hash, first_name, last_name, role, specialization)
            VALUES (@Email, @Hash, @FirstName, @LastName, @Role::user_role, @Specialization)
            RETURNING id
            """;

        var id = await conn.ExecuteScalarAsync<int>(sql, new
        {
            Email          = email,
            Hash           = hash,
            FirstName      = request.FirstName,
            LastName       = request.LastName,
            Role           = request.Role,
            Specialization = request.Specialization
        });

        return new CreateDoctorResponse(
            Id:              id,
            Email:           email,
            Message:         "Cont creat cu succes.",
            DefaultPassword: defaultPassword
        );
    }

    public async Task<(bool Success, string Message)> DeleteDoctorAsync(int doctorId)
    {
        using var conn = _db.CreateConnection();

        // Verifică că există și nu e admin
        var user = await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT id, role FROM users WHERE id = @Id", new { Id = doctorId });

        if (user is null)   return (false, "Doctorul nu a fost găsit.");
        if (user.Role == "admin") return (false, "Nu poți șterge contul de admin.");

        // Dezasociază pacienții înainte de ștergere
        await conn.ExecuteAsync(
            "UPDATE patients SET doctor_id = NULL WHERE doctor_id = @Id", new { Id = doctorId });

        // Șterge notificările
        await conn.ExecuteAsync(
            "DELETE FROM notifications WHERE user_id = @Id", new { Id = doctorId });

        // Șterge contul
        var rows = await conn.ExecuteAsync(
            "DELETE FROM users WHERE id = @Id AND role != 'admin'", new { Id = doctorId });

        return rows > 0
            ? (true,  "Cont șters cu succes.")
            : (false, "Eroare la ștergere.");
    }

    // ── Pacienți ───────────────────────────────────────────

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            SELECT p.id, p.first_name, p.last_name, p.age, p.gender,
                   p.medical_description, p.doctor_id, p.created_at, p.updated_at,
                   u.first_name AS doctor_first_name,
                   u.last_name  AS doctor_last_name,
                   u.specialization AS doctor_specialization
            FROM patients p
            LEFT JOIN users u ON p.doctor_id = u.id
            ORDER BY p.last_name, p.first_name
            """;
        return await conn.QueryAsync<Patient>(sql);
    }

    public async Task<IdResponse> CreatePatientAsync(CreatePatientRequest request)
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            INSERT INTO patients (first_name, last_name, age, gender, medical_description, doctor_id)
            VALUES (@FirstName, @LastName, @Age, @Gender, @MedicalDescription, @DoctorId)
            RETURNING id
            """;
        var id = await conn.ExecuteScalarAsync<int>(sql, request);
        return new IdResponse(id, "Pacient adăugat cu succes.");
    }

    public async Task<(bool Success, string Message)> UpdatePatientAsync(
        int patientId, UpdatePatientRequest request)
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            UPDATE patients
            SET first_name = @FirstName, last_name = @LastName,
                age = @Age, gender = @Gender,
                medical_description = @MedicalDescription,
                doctor_id = @DoctorId, updated_at = NOW()
            WHERE id = @PatientId
            """;
        var rows = await conn.ExecuteAsync(sql, new
        {
            request.FirstName, request.LastName, request.Age,
            request.Gender, request.MedicalDescription, request.DoctorId,
            PatientId = patientId
        });
        return rows > 0
            ? (true,  "Profil actualizat.")
            : (false, "Pacientul nu a fost găsit.");
    }

    public async Task<(bool Success, string Message)> DeletePatientAsync(int patientId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM patients WHERE id = @Id", new { Id = patientId });
        return rows > 0
            ? (true,  "Pacient șters.")
            : (false, "Pacientul nu a fost găsit.");
    }

    // ── Helpers ────────────────────────────────────────────

    private static string RemoveDiacritics(string s) => s
        .Replace("ă", "a").Replace("â", "a").Replace("î", "i")
        .Replace("ș", "s").Replace("ț", "t")
        .Replace("Ă", "a").Replace("Â", "a").Replace("Î", "i")
        .Replace("Ș", "s").Replace("Ț", "t");
}
