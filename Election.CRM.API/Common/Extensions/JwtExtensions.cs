using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

using Election.CRM.API.Features.BlacklistedToken;

namespace Election.CRM.API.Common.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        
        var fromConfig = jwtSettings["SecretKey"];
        var fromEnvUnderscore = Environment.GetEnvironmentVariable("JwtSettings__SecretKey");
        var fromEnvColon = Environment.GetEnvironmentVariable("JwtSettings:SecretKey");
        
        var secretKey = fromConfig ?? fromEnvUnderscore ?? fromEnvColon;

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("ERROR CRÍTICO: La clave secreta de JWT es nula. Revisa los mensajes de DEBUG arriba.");
        }

        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = BuildTokenValidationParameters(jwtSettings, key);
            /* options.Events = BuildJwtEvents(); */
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Intentar leer el token desde la cookie 'auth_token'
                    var token = context.Request.Cookies["auth_token"];
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    private static TokenValidationParameters BuildTokenValidationParameters(IConfigurationSection jwtSettings, byte[] key)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    private static JwtBearerEvents BuildJwtEvents()
    {
        return new JwtBearerEvents
        {
            // 1. NUEVO: Verificar la blacklist DESPUÉS de que el token pasa la validación criptográfica
            OnTokenValidated = async context =>
            {
                // Obtener el identificador único del token (jti)
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("Token inválido: falta el identificador único (jti).");
                    return;
                }

                // Crear un scope para resolver el DbContext de forma segura en middleware
                using var scope = context.HttpContext.RequestServices.CreateScope();
                
                // (ej. RevokedToken, TokenBlacklist, etc., la que mapea a la tabla blacklisted_tokens)
                var dbContext = scope.ServiceProvider.GetRequiredService<Election.CRM.API.Data.CrmContext>();
                
                // Verificar si el jti existe en la tabla de revocación
                // Ajusta "b.Jti" si tu propiedad se llama diferente (ej. b.TokenId, b.JwtId)
                var isBlacklisted = await dbContext.Set<BlacklistedToken>() 
                    .AnyAsync(b => b.Jti == jti);

                if (isBlacklisted)
                {
                    // Rechazar la autenticación si el token fue revocado
                    context.Fail("Token revocado: La sesión ha sido cerrada previamente.");
                }
            },

            // 2. Manejar 401 (No autenticado o token rechazado por la blacklist)
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    statusCode = 401,
                    message = "No autenticado: Debe iniciar sesión o su sesión ha expirado/sido cerrada.",
                    error = "Unauthorized"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            },

            // 3. Manejar 403 (Token válido, pero sin permisos de rol)
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    statusCode = 403,
                    message = "Acceso denegado: No tienes los permisos necesarios para acceder a este recurso.",
                    error = "Forbidden"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };
    }
}