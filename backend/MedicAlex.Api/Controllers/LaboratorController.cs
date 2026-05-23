using MedicAlex.Api.Services;
using MedicAlex.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicAlex.Api.Controllers;

[ApiController]
[Route("api/laborator")]
[Authorize(Roles = "doctor_laborator")]
public class LaboratorController : ControllerBase
{
    private readonly ILaboratorService _service;
    private readonly IJwtService       _jwtService;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration    _config;

    public LaboratorController(
        ILaboratorService service, IJwtService jwtService,
        IWebHostEnvironment env, IConfiguration config)
    {
        _service    = service;
        _jwtService = jwtService;
        _env        = env;
        _config     = config;
    }

    // ════════════════════════════════════════
    //  PACIENȚI (read-only)
    // ════════════════════════════════════════

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients()
        => Ok(await _service.GetAllPatientsAsync());

    [HttpGet("patients/{id:int}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var patient = await _service.GetPatientByIdAsync(id);
        if (patient is null) return NotFound(new MessageResponse("Pacient negăsit."));
        return Ok(patient);
    }

    // ════════════════════════════════════════
    //  CERERI ANALIZE
    // ════════════════════════════════════════

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingRequests()
        => Ok(await _service.GetPendingRequestsAsync());

    [HttpGet("requests/mine")]
    public async Task<IActionResult> GetMyRequests()
    {
        var labDoctorId = _jwtService.GetUserIdFromClaims(User);
        return Ok(await _service.GetMyAcceptedRequestsAsync(labDoctorId));
    }

    [HttpPut("requests/{id:int}/accept")]
    public async Task<IActionResult> AcceptRequest(int id)
    {
        var labDoctorId = _jwtService.GetUserIdFromClaims(User);
        var (success, message) = await _service.AcceptRequestAsync(id, labDoctorId);
        if (!success) return Conflict(new MessageResponse(message));
        return Ok(new MessageResponse(message));
    }

    // ════════════════════════════════════════
    //  UPLOAD PDF REZULTATE
    // ════════════════════════════════════════

    [HttpPost("results")]
    [RequestSizeLimit(20_000_000)]  // max 20MB
    public async Task<IActionResult> UploadResult(
        [FromForm] int patientId,
        [FromForm] IFormFile file,
        [FromForm] string? description,
        [FromForm] int? analysisRequestId)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new MessageResponse("Fișierul PDF este obligatoriu."));

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new MessageResponse("Doar fișiere PDF sunt acceptate."));

        var labDoctorId = _jwtService.GetUserIdFromClaims(User);

        // Creează folderul de upload dacă nu există
        var uploadFolder = Path.Combine(
            _env.ContentRootPath,
            Environment.GetEnvironmentVariable("UPLOAD_PATH") ?? "Uploads");
        Directory.CreateDirectory(uploadFolder);

        // Generează nume unic pentru fișier pe server
        var uniqueName = $"pacient_{patientId}_{labDoctorId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.pdf";
        var filePath   = Path.Combine(uploadFolder, uniqueName);

        // Salvează fișierul
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var (success, message) = await _service.UploadResultAsync(
            labDoctorId:     labDoctorId,
            patientId:       patientId,
            requestId:       analysisRequestId,
            pdfFilename:     uniqueName,
            pdfOriginalName: file.FileName,
            description:     description,
            fileSize:        file.Length
        );

        if (!success) return StatusCode(500, new MessageResponse(message));

        return StatusCode(201, new MessageResponse(message));
    }

    /// <summary>Descarcă un fișier PDF</summary>
    [HttpGet("results/{filename}")]
    public IActionResult DownloadPdf(string filename)
    {
        // Sanitizare — previne directory traversal
        var safeName     = Path.GetFileName(filename);
        var uploadFolder = Path.Combine(
            _env.ContentRootPath,
            Environment.GetEnvironmentVariable("UPLOAD_PATH") ?? "Uploads");
        var filePath     = Path.Combine(uploadFolder, safeName);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new MessageResponse("Fișierul nu a fost găsit."));

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/pdf", safeName);
    }

    [HttpGet("results/patient/{patientId:int}")]
    public async Task<IActionResult> GetPatientResults(int patientId)
        => Ok(await _service.GetPatientResultsAsync(patientId));

    // ════════════════════════════════════════
    //  NOTIFICĂRI
    // ════════════════════════════════════════

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = _jwtService.GetUserIdFromClaims(User);
        return Ok(await _service.GetNotificationsAsync(userId));
    }

    [HttpPut("notifications/{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = _jwtService.GetUserIdFromClaims(User);
        await _service.MarkNotificationReadAsync(id, userId);
        return Ok(new MessageResponse("Notificare marcată ca citită."));
    }

    [HttpPut("notifications/read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _jwtService.GetUserIdFromClaims(User);
        await _service.MarkAllNotificationsReadAsync(userId);
        return Ok(new MessageResponse("Toate notificările marcate ca citite."));
    }

    // ════════════════════════════════════════
    //  DOCTORI (vizualizare)
    // ════════════════════════════════════════

    [HttpGet("doctors/medicina")]
    public async Task<IActionResult> GetMedicinaDoctors()
    {
        // Reutilizăm query-ul din MedicinaService prin DI
        using var scope   = HttpContext.RequestServices.CreateScope();
        var medService    = scope.ServiceProvider.GetRequiredService<IMedicinaService>();
        return Ok(await medService.GetAllMedicinaDoctorsAsync());
    }

    [HttpGet("doctors/laborator")]
    public async Task<IActionResult> GetLaboratorDoctors()
    {
        using var scope   = HttpContext.RequestServices.CreateScope();
        var medService    = scope.ServiceProvider.GetRequiredService<IMedicinaService>();
        return Ok(await medService.GetAllLaboratorDoctorsAsync());
    }
}
