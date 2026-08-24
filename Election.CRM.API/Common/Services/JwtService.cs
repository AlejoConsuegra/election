using Election.CRM.API.Features.User;
using Election.CRM.API.Features.User.DTOs;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Election.CRM.API.Common.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse GenerateLoginResponse(User user, string roleName, string refreshToken)
    {
        var (jwtSettings, credentials) = GetConfigurationJWT(_configuration);

        var expiration = CalculateExpirationOnlyOne(jwtSettings);

        var claims = CreateClaim(user, roleName);
        
        var token = CreateToken(jwtSettings, claims, expiration, credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Nombre = user.Nombre,
            Email = user.Email,
            Rol = roleName,
            Expiration = expiration
        };
    }

    private (IConfigurationSection jwtSettings, SigningCredentials credentials) GetConfigurationJWT(IConfiguration configuration){
            var jwtSettings = configuration.GetSection("JwtSettings");
    
            // Intenta leer de la configuración. Si es null, lee directamente de la variable de entorno
            var secretKey = jwtSettings["SecretKey"] ?? Environment.GetEnvironmentVariable("JwtSettings__SecretKey");
            
            // Validación explícita
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("ERROR CRÍTICO: La clave secreta de JWT es nula en JwtService. Verifica appsettings.json y docker-compose.yml");
            }
            
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return (jwtSettings, credentials);
    } 

    private DateTime CalculateExpirationOnlyOne(IConfigurationSection jwtSettings){
        var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"]!);
        return DateTime.UtcNow.AddMinutes(expirationMinutes);
    }

    private Claim[] CreateClaim(User user, string roleName)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Role, roleName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return claims;
    }

    private JwtSecurityToken CreateToken(
        IConfigurationSection jwtSettings, 
        Claim[] claims, 
        DateTime expiration,              
        SigningCredentials credentials)    
    {
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return token;
    }
}