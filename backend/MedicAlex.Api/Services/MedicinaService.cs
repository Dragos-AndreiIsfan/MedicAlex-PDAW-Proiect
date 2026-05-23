using Dapper;
using MedicAlex.Api.Data;
using MedicAlex.Api.DTOs;
using MedicAlex.Api.Models;

namespace MedicAlex.Api.Services;

public interface IMedicinaService
{
    Task<IEnumerable<Patient>>         GetMyPatientsAsync(int doctorId);
    Task<IEnumerable<Patient>>         GetOtherPatientsAsync(int doctorId);
    Task<Patient?>                     GetPatientByIdAsync(int patientId);
    Task<IdResponse>                   CreatePatientAsync(int doctorId, CreateMyPatientRequest request);
    Task<(bool, string)>               UpdatePatientAsync(int doctorId, int patientId, CreateMyPatientRequest request);
    Task<(bool, string)>               DeletePatientAsync(int doctorId, int patientId);
    Task<IdResponse>                   CreateAnalysisRequestAsync(int doctorId, CreateAnalysisRequestDto request);
    Task<IEnumerable<AnalysisRequest>> GetPatientRequestsAsync(int patientId);
    Task<IEnumerable<AnalysisResult>>  GetPatientResultsAsync(int patientId);
    Task<IEnumerable<User>>            GetAllMedicinaDoctorsAsync();
    Task<IEnumerable<User>>            GetAllLaboratorDoctorsAsync();
}

public class MedicinaService : IMedicinaService
{
    private readonly DatabaseContext _db;

    public MedicinaService(DatabaseContext db) => _db = db;

    private const string PatientSelectSql = """
        SELECT p.id, p.first_name, p.last_name, p.age, p.gender,
               p.medical_description, p.doctor_id, p.created_at, p.updated_at,
               u.first_name AS doctor_first_name,
               u.last_name  AS doctor_last_name,
               u.specialization AS doctor_specialization
        FROM patients p
        LEFT JOIN users u ON p.doctor_id = u.id
        """;

    public async Task<IEnumerable<Patient>> GetMyPatientsAsync(int doctorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Patient>(
            PatientSelectSql + " WHERE p.doctor_id = @DoctorId ORDER BY p.last_name",
            new { DoctorId = doctorId });
    }

    public async Task<IEnumerable<Patient>> GetOtherPatientsAsync(int doctorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Patient>(
            PatientSelectSql + " WHERE (p.doctor_id != @DoctorId OR p.doctor_id IS NULL) ORDER BY p.last_name",
            new { DoctorId = doctorId });
    }

    public async Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Patient>(
            PatientSelectSql + " WHERE p.id = @PatientId",
            new { PatientId = patientId });
    }

    public async Task<IdResponse> CreatePatientAsync(int doctorId, CreateMyPatientRequest request)
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            INSERT INTO patients (first_name, last_name, age, gender, medical_description, doctor_id)
            VALUES (@FirstName, @LastName, @Age, @Gender, @MedicalDescription, @DoctorId)
            RETURNING id
            """;
        var id = await conn.ExecuteScalarAsync<int>(sql, new
        {
            request.FirstName, request.LastName, request.Age,
            request.Gender, request.MedicalDescription, DoctorId = doctorId
        });
        return new IdResponse(id, "Pacient adăugat cu succes.");
    }

    public async Task<(bool, string)> UpdatePatientAsync(
        int doctorId, int patientId, CreateMyPatientRequest request)
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            UPDATE patients
            SET first_name = @FirstName, last_name = @LastName,
                age = @Age, gender = @Gender,
                medical_description = @MedicalDescription,
                updated_at = NOW()
            WHERE id = @PatientId AND doctor_id = @DoctorId
            """;
        var rows = await conn.ExecuteAsync(sql, new
        {
            request.FirstName, request.LastName, request.Age,
            request.Gender, request.MedicalDescription,
            PatientId = patientId, DoctorId = doctorId
        });
        return rows > 0
            ? (true,  "Profil actualizat.")
            : (false, "Pacientul nu a fost găsit sau nu aveți acces.");
    }

    public async Task<(bool, string)> DeletePatientAsync(int doctorId, int patientId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.ExecuteAsync(
            "DELETE FROM patients WHERE id = @PatientId AND doctor_id = @DoctorId",
            new { PatientId = patientId, DoctorId = doctorId });
        return rows > 0
            ? (true,  "Pacient șters.")
            : (false, "Pacientul nu a fost găsit sau nu aveți acces.");
    }

    public async Task<IdResponse> CreateAnalysisRequestAsync(
        int doctorId, CreateAnalysisRequestDto request)
    {
        using var conn = _db.CreateConnection();

        // Verifică că pacientul aparține acestui doctor
        var owned = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM patients WHERE id = @PatientId AND doctor_id = @DoctorId",
            new { request.PatientId, DoctorId = doctorId });
        if (owned == 0)
            throw new UnauthorizedAccessException("Nu aveți acces la acest pacient.");

        // Inserează cererea
        const string sql = """
            INSERT INTO analysis_requests (patient_id, requesting_doctor_id, analysis_type)
            VALUES (@PatientId, @DoctorId, @AnalysisType)
            RETURNING id
            """;
        var requestId = await conn.ExecuteScalarAsync<int>(sql, new
        {
            request.PatientId, DoctorId = doctorId, request.AnalysisType
        });

        // Obține date pentru notificare
        var patientName = await conn.ExecuteScalarAsync<string>(
            "SELECT first_name || ' ' || last_name FROM patients WHERE id = @Id",
            new { Id = request.PatientId });
        var doctorName = await conn.ExecuteScalarAsync<string>(
            "SELECT first_name || ' ' || last_name FROM users WHERE id = @Id",
            new { Id = doctorId });

        // Trimite notificare tuturor doctorilor laborator
        var labDoctors = await conn.QueryAsync<int>(
            "SELECT id FROM users WHERE role = 'doctor_laborator' AND is_active = TRUE");

        var message = $"Cerere analiză nouă: {request.AnalysisType} " +
                      $"pentru pacientul {patientName} (Dr. {doctorName})";

        foreach (var labId in labDoctors)
        {
            await conn.ExecuteAsync("""
                INSERT INTO notifications (user_id, message, related_request_id)
                VALUES (@UserId, @Message, @RequestId)
                """, new { UserId = labId, Message = message, RequestId = requestId });
        }

        return new IdResponse(requestId, "Cerere trimisă cu succes.");
    }

    public async Task<IEnumerable<AnalysisRequest>> GetPatientRequestsAsync(int patientId)
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            SELECT ar.id, ar.patient_id, ar.requesting_doctor_id, ar.analysis_type,
                   ar.status, ar.accepted_by, ar.created_at, ar.updated_at,
                   p.first_name  AS patient_first_name,
                   p.last_name   AS patient_last_name,
                   d.first_name  AS requesting_doctor_first_name,
                   d.last_name   AS requesting_doctor_last_name,
                   d.specialization AS requesting_doctor_spec,
                   l.first_name  AS accepted_by_first_name,
                   l.last_name   AS accepted_by_last_name
            FROM analysis_requests ar
            JOIN  patients p ON ar.patient_id           = p.id
            JOIN  users    d ON ar.requesting_doctor_id = d.id
            LEFT JOIN users l ON ar.accepted_by         = l.id
            WHERE ar.patient_id = @PatientId
            ORDER BY ar.created_at DESC
            """;
        return await conn.QueryAsync<AnalysisRequest>(sql, new { PatientId = patientId });
    }

    public async Task<IEnumerable<AnalysisResult>> GetPatientResultsAsync(int patientId)
    {
        using var conn = _db.CreateConnection();
        const string sql = """
            SELECT ar.id, ar.patient_id, ar.analysis_request_id,
                   ar.lab_doctor_id, ar.pdf_filename, ar.pdf_original_name,
                   ar.description, ar.file_size_bytes, ar.uploaded_at,
                   u.first_name AS lab_doctor_first_name,
                   u.last_name  AS lab_doctor_last_name
            FROM analysis_results ar
            JOIN users u ON ar.lab_doctor_id = u.id
            WHERE ar.patient_id = @PatientId
            ORDER BY ar.uploaded_at DESC
            """;
        return await conn.QueryAsync<AnalysisResult>(sql, new { PatientId = patientId });
    }

    public async Task<IEnumerable<User>> GetAllMedicinaDoctorsAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(
            "SELECT id, email, first_name, last_name, role, specialization FROM users WHERE role = 'doctor_medicina' AND is_active = TRUE ORDER BY last_name");
    }

    public async Task<IEnumerable<User>> GetAllLaboratorDoctorsAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>(
            "SELECT id, email, first_name, last_name, role, specialization FROM users WHERE role = 'doctor_laborator' AND is_active = TRUE ORDER BY last_name");
    }
}
