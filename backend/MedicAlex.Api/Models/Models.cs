namespace MedicAlex.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? MedicalDescription { get; set; }
    public int? DoctorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Joined fields
    public string? DoctorFirstName { get; set; }
    public string? DoctorLastName { get; set; }
    public string? DoctorSpecialization { get; set; }
    public string DoctorName => DoctorFirstName != null
        ? $"{DoctorFirstName} {DoctorLastName}"
        : "Neasignat";
}

public class AnalysisRequest
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int RequestingDoctorId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public int? AcceptedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Joined fields
    public string? PatientFirstName { get; set; }
    public string? PatientLastName { get; set; }
    public string? RequestingDoctorFirstName { get; set; }
    public string? RequestingDoctorLastName { get; set; }
    public string? RequestingDoctorSpec { get; set; }
    public string? AcceptedByFirstName { get; set; }
    public string? AcceptedByLastName { get; set; }

    public string PatientName => $"{PatientFirstName} {PatientLastName}";
    public string RequestingDoctorName => $"{RequestingDoctorFirstName} {RequestingDoctorLastName}";
    public string? AcceptedByName => AcceptedByFirstName != null
        ? $"Acceptat de Dr. {AcceptedByFirstName} {AcceptedByLastName}"
        : null;
}

public class AnalysisResult
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? AnalysisRequestId { get; set; }
    public int LabDoctorId { get; set; }
    public string PdfFilename { get; set; } = string.Empty;
    public string PdfOriginalName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }

    // Joined fields
    public string? LabDoctorFirstName { get; set; }
    public string? LabDoctorLastName { get; set; }
    public string LabDoctorName => $"Dr. {LabDoctorFirstName} {LabDoctorLastName}";
}

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? RelatedRequestId { get; set; }
    public DateTime CreatedAt { get; set; }
}
