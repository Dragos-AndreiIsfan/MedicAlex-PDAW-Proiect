using Dapper;
using MedicAlex.Api.Data;
using MedicAlex.Api.Models;

namespace MedicAlex.Api.Services;

public interface ILaboratorService
{
    Task<IEnumerable<Patient>>         GetAllPatientsAsync();
    Task<Patient?>                     GetPatientByIdAsync(int patientId);
    Task<IEnumerable<AnalysisRequest>> GetPendingRequestsAsync();
    Task<IEnumerable<AnalysisRequest>> GetMyAcceptedRequestsAsync(int labDoctorId);
    Task<(bool, string)>               AcceptRequestAsync(int requestId, int labDoctorId);
    Task<(bool, string)>               UploadResultAsync(int labDoctorId, int patientId, int? requestId, string pdfFilename, string pdfOriginalName, string? description, long fileSize);
    Task<IEnumerable<AnalysisResult>>  GetPatientResultsAsync(int patientId);
    Task<IEnumerable<Notification>>    GetNotificationsAsync(int userId);
    Task                               MarkNotificationReadAsync(int notificationId, int userId);
    Task                               MarkAllNotificationsReadAsync(int userId);
}

public class LaboratorService : ILaboratorService
{
    private readonly DatabaseContext _db;

    public LaboratorService(DatabaseContext db) => _db = db;

    private const string PatientSelectSql = """
        SELECT p.id, p.first_name, p.last_name, p.age, p.gender,
               p.medical_description, p.doctor_id, p.created_at, p.updated_at,
               u.first_name AS doctor_first_name,
               u.last_name  AS doctor_last_name,
               u.specialization AS doctor_specialization
        FROM patients p
        LEFT JOIN users u ON p.doctor_id = u.id
        """;

    private const string RequestSelectSql = """
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
        """;

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Patient>(
            PatientSelectSql + " ORDER BY p.last_name, p.first_name");
    }

    public async Task<Patient?> GetPatientByIdAsync(int patientId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Patient>(
            PatientSelectSql + " WHERE p.id = @PatientId",
            new { PatientId = patientId });
    }

    public async Task<IEnumerable<AnalysisRequest>> GetPendingRequestsAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<AnalysisRequest>(
            RequestSelectSql + " WHERE ar.status = 'pending' ORDER BY ar.created_at DESC");
    }

    public async Task<IEnumerable<AnalysisRequest>> GetMyAcceptedRequestsAsync(int labDoctorId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<AnalysisRequest>(
            RequestSelectSql + " WHERE ar.accepted_by = @LabDoctorId AND ar.status IN ('accepted','completed') ORDER BY ar.updated_at DESC",
            new { LabDoctorId = labDoctorId });
    }

    public async Task<(bool, string)> AcceptRequestAsync(int requestId, int labDoctorId)
    {
        using var conn = _db.CreateConnection();

        // Acceptă doar dacă e pending — concurență: nu doi doctori să accepte simultan
        const string updateSql = """
            UPDATE analysis_requests
            SET status = 'accepted', accepted_by = @LabDoctorId, updated_at = NOW()
            WHERE id = @RequestId AND status = 'pending'
            """;
        var rows = await conn.ExecuteAsync(updateSql,
            new { RequestId = requestId, LabDoctorId = labDoctorId });

        if (rows == 0)
            return (false, "Cererea nu mai este disponibilă (deja acceptată de alt doctor).");

        // Șterge notificările tuturor celorlalți doctori laborator pentru această cerere
        await conn.ExecuteAsync(
            "DELETE FROM notifications WHERE related_request_id = @RequestId",
            new { RequestId = requestId });

        return (true, "Cerere acceptată cu succes.");
    }

    public async Task<(bool, string)> UploadResultAsync(
        int labDoctorId, int patientId, int? requestId,
        string pdfFilename, string pdfOriginalName,
        string? description, long fileSize)
    {
        using var conn = _db.CreateConnection();

        const string sql = """
            INSERT INTO analysis_results
                (patient_id, analysis_request_id, lab_doctor_id,
                 pdf_filename, pdf_original_name, description, file_size_bytes)
            VALUES
                (@PatientId, @RequestId, @LabDoctorId,
                 @PdfFilename, @PdfOriginalName, @Description, @FileSize)
            RETURNING id
            """;

        var id = await conn.ExecuteScalarAsync<int>(sql, new
        {
            PatientId      = patientId,
            RequestId      = requestId,
            LabDoctorId    = labDoctorId,
            PdfFilename    = pdfFilename,
            PdfOriginalName = pdfOriginalName,
            Description    = description,
            FileSize       = fileSize
        });

        // Marchează cererea ca finalizată dacă e asociată
        if (requestId.HasValue)
        {
            await conn.ExecuteAsync("""
                UPDATE analysis_requests
                SET status = 'completed', updated_at = NOW()
                WHERE id = @RequestId AND accepted_by = @LabDoctorId
                """, new { RequestId = requestId, LabDoctorId = labDoctorId });
        }

        return (true, "Rezultate încărcate cu succes.");
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

    public async Task<IEnumerable<Notification>> GetNotificationsAsync(int userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Notification>(
            "SELECT * FROM notifications WHERE user_id = @UserId ORDER BY created_at DESC",
            new { UserId = userId });
    }

    public async Task MarkNotificationReadAsync(int notificationId, int userId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE notifications SET is_read = TRUE WHERE id = @Id AND user_id = @UserId",
            new { Id = notificationId, UserId = userId });
    }

    public async Task MarkAllNotificationsReadAsync(int userId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE notifications SET is_read = TRUE WHERE user_id = @UserId",
            new { UserId = userId });
    }
}
