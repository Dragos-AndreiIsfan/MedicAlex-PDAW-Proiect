using MedicAlex.Api.DTOs;
using MedicAlex.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicAlex.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]  // Orice utilizator autentificat poate schimba parola
public class ProfileController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService  _jwtService;

    public ProfileController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService  = jwtService;
    }

    /// <summary>
    /// Schimbare parolă — disponibilă pentru TOȚI utilizatorii autentificați:
    /// admin, doctor_medicina, doctor_laborator
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = _jwtService.GetUserIdFromClaims(User);
        var (success, message) = await _authService.ChangePasswordAsync(userId, request);

        if (!success)
            return BadRequest(new MessageResponse(message));

        return Ok(new MessageResponse(message));
    }
}
