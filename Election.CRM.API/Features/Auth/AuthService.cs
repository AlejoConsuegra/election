using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
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

    public AuthService(
        CrmContext context, 
        JwtService jwtService,
        CookieService cookieService,
        LoginAttemptService loginAttemptService,
        RefreshTokenService refreshTokenService,
        ITokenBlacklistService blacklistService    
    )
    {
        _context = context;
        _jwtService = jwtService;
        _cookieService = cookieService;
        _loginAttemptService = loginAttemptService;
        _refreshTokenService = refreshTokenService;
        _blacklistService = blacklistService;
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

    public async Task RevokeAllUserRefreshTokensAsync(string userId)
    {
        // Marca como revocados todos los refresh tokens activos de este usuario
        /* await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.IsRevoked, true)); */
    }

    public async Task CompleteLogoutAsync(string userId, string jti, DateTime expiresAt)
    {
        // 1. Revocar el Access Token en la base de datos (Blacklist)
        await _blacklistService.RevokeAsync(jti, userId, expiresAt, "logout");

        // 2. Revocar TODOS los Refresh Tokens de este usuario en memoria
        // (Usamos el método que ya tienes en tu RefreshTokenService)
        _refreshTokenService.RevokeAllUserTokens(int.Parse(userId));

        // 3. Limpiar las cookies del navegador
        _cookieService.ClearAuthCookies(); // Asegúrate de que este método exista en tu CookieService
    }

    public void ClearAuthCookies()
    {
        _cookieService.ClearAuthCookies();
    }
}