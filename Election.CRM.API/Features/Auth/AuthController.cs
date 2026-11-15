using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Election.CRM.API.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Election.CRM.API.Data;

using Election.CRM.API.Common.DTOs; 
using Election.CRM.API.Common.Exceptions;
using Election.CRM.API.Common.Extensions; 

using Election.CRM.API.Features.Auth.DTOs;
using Election.CRM.API.Features.BlacklistedToken.Services;
using System.Threading.Tasks;

namespace Election.CRM.API.Features.Auth;

[Route("api/[controller]")]
[ApiController]
[Authorize] 
[Tags("Auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenBlacklistService _blacklistService;
    private readonly AuthService _authService;

    public AuthController(ITokenBlacklistService blacklistService, AuthService authService)
    {
        _blacklistService = blacklistService;
        _authService = authService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        // El controlador solo delega. El middleware de JWT ya validó el token antes de llegar aquí.
        var profile = await _authService.GetCurrentUserProfile();
        
        return this.Success(profile, "Perfil obtenido correctamente.", 200);
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [AllowAnonymous] 
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var response = await _authService.LoginAsync(request, ipAddress);
        
        return this.Success(response, "Inicio de sesión exitoso.", 200);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        
        return this.Success(response, "Token renovado correctamente.", 200);
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.CompleteLogoutAsync(User);
        
        return this.Success<object>(null, "Sesión cerrada correctamente.", 200);
    }
    
    [HttpPost("blacklist/purge")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PurgeBlacklist()
    {
        var purged = await _blacklistService.PurgeExpiredAsync();
        
        var responseData = new PurgeBlacklistResponse
        {
            PurgedCount = purged,
            ExecutedAt = DateTime.UtcNow
        };

        return this.Success(responseData, $"Purga ejecutada correctamente. {purged} tokens eliminados.", 200);
    }
}
