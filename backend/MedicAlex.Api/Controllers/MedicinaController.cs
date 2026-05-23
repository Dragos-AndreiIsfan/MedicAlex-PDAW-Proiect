using MedicAlex.Api.DTOs;
using MedicAlex.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicAlex.Api.Controllers;

[ApiController]
[Route("api/medicina")]
[Authorize(Roles = "doctor_medicina")]
public class MedicinaController : ControllerBase
{
    private readonly IMedicinaService _service;
    private readonly IJwtService      _jwtService;

    public MedicinaController(IMedicinaService service, IJwtService jwtService)
    {
        _service    = service;
        _jwtService = jwtService;
    }

    // ════════════════════════════════════════
    //  PACIENȚII MEI
    // ════════════════════════════════════════

    [HttpGet("patients/mine")]
    public async Task<IActionResult> GetMyPatients()
    {
        var doctorId = _jwtService.GetUserIdFromClaims(User);
        return Ok(await _service.GetMyPatientsAsync(doctorId));
    }

    [HttpGet("patients/others")]
    public async Task<IActionResult> GetOtherPatients()
    {
        var doctorId = _jwtService.GetUserIdFromClaims(User);
        return Ok(await _service.GetOtherPatientsAsync(doctorId));
    }

    [HttpGet("patients/{id:int}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var patient = await _service.GetPatientByIdAsync(id);
        if (patient is null) return NotFound(new MessageResponse("Pacient negăsit."));
        return Ok(patient);
    }

    [HttpPost("patients")]
    public async Task<IActionResult> CreatePatient([FromBody] CreateMyPatientRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var doctorId = _jwtService.GetUserIdFromClaims(User);
        var result   = await _service.CreatePatientAsync(doctorId, request);
        return StatusCode(201, result);
    }

    [HttpPut("patients/{id:int}")]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] CreateMyPatientRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var doctorId = _jwtService.GetUserIdFromClaims(User);
        var (success, message) = await _service.UpdatePatientAsync(doctorId, id, request);
        if (!success) return NotFound(new MessageResponse(message));
        return Ok(new MessageResponse(message));
    }

    [HttpDelete("patients/{id:int}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var doctorId = _jwtService.GetUserIdFromClaims(User);
        var (success, message) = await _service.DeletePatientAsync(doctorId, id);
        if (!success) return NotFound(new MessageResponse(message));
        return Ok(new MessageResponse(message));
    }

    // ════════════════════════════════════════
    //  CERERI ANALIZE
    // ════════════════════════════════════════

    [HttpPost("analysis-requests")]
    public async Task<IActionResult> CreateAnalysisRequest([FromBody] CreateAnalysisRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var doctorId = _jwtService.GetUserIdFromClaims(User);
            var result   = await _service.CreateAnalysisRequestAsync(doctorId, request);
            return StatusCode(201, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    [HttpGet("analysis-requests/patient/{patientId:int}")]
    public async Task<IActionResult> GetPatientRequests(int patientId)
        => Ok(await _service.GetPatientRequestsAsync(patientId));

    [HttpGet("results/patient/{patientId:int}")]
    public async Task<IActionResult> GetPatientResults(int patientId)
        => Ok(await _service.GetPatientResultsAsync(patientId));

    // ════════════════════════════════════════
    //  DOCTORI (vizualizare)
    // ════════════════════════════════════════

    [HttpGet("doctors/medicina")]
    public async Task<IActionResult> GetMedicinaDoctors()
        => Ok(await _service.GetAllMedicinaDoctorsAsync());

    [HttpGet("doctors/laborator")]
    public async Task<IActionResult> GetLaboratorDoctors()
        => Ok(await _service.GetAllLaboratorDoctorsAsync());
}
