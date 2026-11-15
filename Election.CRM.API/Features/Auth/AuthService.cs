using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Election.CRM.API.Common.Extensions; 
using Election.CRM.API.Common.Exceptions;

using Election.CRM.API.Data;
using Election.CRM.API.Common.Services;
using Election.CRM.API.Common.Helpers;
using Election.CRM.API.Common.DTOs; 

using Election.CRM.API.Features.User.DTOs;
using Election.CRM.API.Features.Auth.DTOs;
using Election.CRM.API.Features.BlacklistedToken.Services;


namespace Election.CRM.API.Features.Auth;

public class AuthService
{
    private readonly CrmContext _context;
    private readonly JwtService _jwtService;
    private readonly CookieService _cookieService;
    private readonly LoginAttemptService _loginAttemptService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly ITokenBlacklistService _blacklistService;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        CrmContext context, 
        JwtService jwtService,
        CookieService cookieService,
        LoginAttemptService loginAttemptService,
        RefreshTokenService refreshTokenService,
        ITokenBlacklistService blacklistService,
        ICurrentUserService currentUserService 
    )
    {
        _context = context;
        _jwtService = jwtService;
        _cookieService = cookieService;
        _loginAttemptService = loginAttemptService;
        _refreshTokenService = refreshTokenService;
        _blacklistService = blacklistService;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> GetCurrentUserProfile()
    {
        if (_currentUserService.UserId == 0)
        {
            throw new UnauthorizedAccessException("No se pudo identificar al usuario en el token.");
        }

        return new UserProfileDto
        {
            Id = _currentUserService.UserId,
            Nombre = _currentUserService.Nombre,
            Email = _currentUserService.Email,
            Rol = _currentUserService.RoleName
        };
    }

    public async Task<LoginResponseData> LoginAsync(LoginRequest request, string ipAddress)
    {
        if (request == null) 
        throw new ArgumentNullException(nameof(request), "El objeto LoginRequest llegó como null desde el Controller.");
        
        // 1. Verificar si la IP está bloqueada
        if (_loginAttemptService.IsLockedOut(ipAddress))
        {
            var lockoutUntil = _loginAttemptService.GetLockoutUntil(ipAddress);
            var minutesRemaining = lockoutUntil.HasValue 
                ? (int)(lockoutUntil.Value - DateTime.UtcNow).TotalMinutes + 1 
                : 15;
            
            throw new InvalidOperationException(
                $"Cuenta bloqueada temporalmente. Intente nuevamente en {minutesRemaining} minutos.");
        }

        // 2. Buscar el usuario
        var cedula = request.Cedula;
        var user = await _context.Users
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Cedula == cedula);

        // 3. Validar credenciales
        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.Password))
        {
            _loginAttemptService.RecordFailedAttempt(ipAddress);
            var remaining = _loginAttemptService.GetRemainingAttempts(ipAddress);
            throw new UnauthorizedAccessException(
                $"Credenciales inválidas. Intentos restantes: {remaining}");
        }

        // 4. Login exitoso, resetear intentos
        _loginAttemptService.ResetAttempts(ipAddress);

        // 5. Generar refresh token
        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        _refreshTokenService.SaveRefreshToken(refreshToken, user.Id);

        // 6. Generar respuesta con ambos tokens
        var jwtResponse = _jwtService.GenerateLoginResponse(user, user.IdRolNavigation.NameRol, refreshToken);

        _cookieService.SetAuthCookies(jwtResponse.Token, jwtResponse.RefreshToken, jwtResponse.Expiration);

        return new LoginResponseData
        {
            Nombre = user.Nombre,
            Email = user.Email,
            Rol = jwtResponse.Rol
        };
    }
    
    public async Task<LoginResponseData> RefreshTokenAsync(string refreshToken)
    {
        // 1. Validar el refresh token
        var tokenInfo = _refreshTokenService.ValidateRefreshToken(refreshToken);
        if (tokenInfo == null)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado");

        // 2. Buscar el usuario
        var user = await _context.Users
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Id == tokenInfo.UserId);

        if (user == null)
            throw new UnauthorizedAccessException("Usuario no encontrado");

        // 3. Revocar el refresh token anterior (rotación)
        _refreshTokenService.RevokeRefreshToken(refreshToken);

        // 4. Generar nuevo refresh token
        var newRefreshToken = _refreshTokenService.GenerateRefreshToken();
        _refreshTokenService.SaveRefreshToken(newRefreshToken, user.Id);

        // 5. Generar nuevo access token
        var jwtResponse = _jwtService.GenerateLoginResponse(user, user.IdRolNavigation.NameRol, refreshToken);

        _cookieService.SetAuthCookies(jwtResponse.Token, jwtResponse.RefreshToken, jwtResponse.Expiration);

        return new LoginResponseData
        {
            Nombre = user.Nombre,
            Email = user.Email,
            Rol = jwtResponse.Rol
        };
    }

    public Task RevokeAllUserRefreshTokensAsync(string userId)
    {
        // Marca como revocados todos los refresh tokens activos de este usuario
        /* await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.IsRevoked, true)); */

        return Task.CompletedTask;
    }

    public async Task CompleteLogoutAsync(ClaimsPrincipal user)
    {
        var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
        {
            throw new InvalidOperationException("No se pudo identificar el token o el usuario para la revocación.");
        }

        var expClaim = user.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        DateTime expiresAt;
        
        if (long.TryParse(expClaim, out long expTimestamp))
        {
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expTimestamp).UtcDateTime;
        }
        else
        {
            // Fallback de seguridad
            expiresAt = DateTime.UtcNow.AddMinutes(30);
        }

        // Revocar el Access Token en la base de datos (Blacklist)
        await _blacklistService.RevokeAsync(jti, userId, expiresAt, "logout");

        // Revocar TODOS los Refresh Tokens de este usuario
        _refreshTokenService.RevokeAllUserTokens(int.Parse(userId));

        // Limpiar las cookies del navegador
        _cookieService.ClearAuthCookies(); 
    }

    public void ClearAuthCookies()
    {
        _cookieService.ClearAuthCookies();
    }
}