using MedicAlex.Api.DTOs;
using MedicAlex.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicAlex.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
        => _adminService = adminService;

    // ════════════════════════════════════════
    //  DOCTORI
    // ════════════════════════════════════════

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors()
        => Ok(await _adminService.GetAllDoctorsAsync());

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _adminService.CreateDoctorAsync(request);
            return StatusCode(201, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new MessageResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new MessageResponse(ex.Message));
        }
    }

    [HttpDelete("doctors/{id:int}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var (success, message) = await _adminService.DeleteDoctorAsync(id);
        if (!success) return NotFound(new MessageResponse(message));
        return Ok(new MessageResponse(message));
    }

    // ════════════════════════════════════════
    //  PACIENȚI
    // ════════════════════════════════════════

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients()
        => Ok(await _adminService.GetAllPatientsAsync());

    [HttpPost("patients")]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _adminService.CreatePatientAsync(request);
        return StatusCode(201, result);
    }

    [HttpPut("patients/{id:int}")]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var (success, message) = await _adminService.UpdatePatientAsync(id, request);
        if (!success) return NotFound(new MessageResponse(message));
        return Ok(new MessageResponse(message));
    }

    [HttpDelete("patients/{id:int}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var (success, message) = await _adminService.DeletePatientAsync(id);
        if (!success) return NotFound(new MessageResponse(message));
        return Ok(new MessageResponse(message));
    }
}
