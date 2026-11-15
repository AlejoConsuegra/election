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
        // 1. Extraer claims del token (ya validado por el middleware de JWT)
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
        {
            return this.Error("No se pudo identificar el token o el usuario para la revocación.", 400);
        }

        // 2. Extraer la fecha de expiración del claim 'exp' (Unix timestamp)
        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        DateTime expiresAt;
        
        if (long.TryParse(expClaim, out long expTimestamp))
        {
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expTimestamp).UtcDateTime;
        }
        else
        {
            // Fallback de seguridad: si no hay claim 'exp', asumimos 30 minutos desde ahora
            expiresAt = DateTime.UtcNow.AddMinutes(30);
        }

        // 3. Delegar toda la lógica de negocio al servicio
        await _authService.CompleteLogoutAsync(userId, jti, expiresAt);

        // 4. Retornar respuesta exitosa
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
