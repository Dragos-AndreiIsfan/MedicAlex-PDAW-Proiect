using MedicAlex.Api.DTOs;
using MedicAlex.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicAlex.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService  _jwtService;

    public AuthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService  = jwtService;
    }

    /// <summary>Login — returnează JWT token</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(new MessageResponse("Email sau parolă incorectă."));

        return Ok(result);
    }

    /// <summary>Returnează profilul utilizatorului curent</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = _jwtService.GetUserIdFromClaims(User);
        var profile = await _authService.GetProfileAsync(userId);
        if (profile is null) return NotFound();
        return Ok(profile);
    }
}
