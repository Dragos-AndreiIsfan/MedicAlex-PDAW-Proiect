using System.ComponentModel.DataAnnotations;

namespace MedicAlex.Api.DTOs;

// ══════════════════════════════════════════
//  AUTH
// ══════════════════════════════════════════

public record LoginRequest(
    [Required] string Email,
    [Required] string Password
);

public record LoginResponse(
    string Token,
    UserDto User
);

public record UserDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string? Specialization
);

// ══════════════════════════════════════════
//  PROFILE / SCHIMBARE PAROLĂ
// ══════════════════════════════════════════

public record ChangePasswordRequest(
    [Required, MinLength(1)] string CurrentPassword,
    [Required, MinLength(6)] string NewPassword,
    [Required] string ConfirmNewPassword
);

// ══════════════════════════════════════════
//  ADMIN — DOCTORI
// ══════════════════════════════════════════

public record CreateDoctorRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required] string Role,               // "doctor_medicina" | "doctor_laborator"
    [MaxLength(150)] string? Specialization
);

public record CreateDoctorResponse(
    int Id,
    string Email,
    string Message,
    string DefaultPassword
);

// ══════════════════════════════════════════
//  ADMIN / MEDICINA — PACIENȚI
// ══════════════════════════════════════════

public record CreatePatientRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, Range(1, 149)] int Age,
    [Required] string Gender,
    string? MedicalDescription,
    int? DoctorId
);

public record UpdatePatientRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, Range(1, 149)] int Age,
    [Required] string Gender,
    string? MedicalDescription,
    int? DoctorId
);

// Doctor Medicina — pacient propriu (fără DoctorId în body, vine din JWT)
public record CreateMyPatientRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, Range(1, 149)] int Age,
    [Required] string Gender,
    string? MedicalDescription
);

// ══════════════════════════════════════════
//  MEDICINA — CERERI ANALIZE
// ══════════════════════════════════════════

public record CreateAnalysisRequestDto(
    [Required] int PatientId,
    [Required, MaxLength(255)] string AnalysisType
);

// ══════════════════════════════════════════
//  RESPONSE-URI COMUNE
// ══════════════════════════════════════════

public record MessageResponse(string Message);

public record IdResponse(int Id, string Message);
