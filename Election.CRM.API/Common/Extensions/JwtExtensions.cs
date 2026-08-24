using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Election.CRM.API.Common.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        
        // Intentamos leer la clave de 3 formas diferentes
        var fromConfig = jwtSettings["SecretKey"];
        var fromEnvUnderscore = Environment.GetEnvironmentVariable("JwtSettings__SecretKey");
        var fromEnvColon = Environment.GetEnvironmentVariable("JwtSettings:SecretKey");
        
        // Usamos la primera que no sea nula
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
            options.Events = BuildJwtEvents();
        });

        return services;
    }

    /// <summary>
    /// Construye los parámetros de validación del token JWT
    /// </summary>
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

    /// <summary>
    /// Construye los eventos personalizados para manejar respuestas 401 y 403
    /// </summary>
    private static JwtBearerEvents BuildJwtEvents()
    {
        return new JwtBearerEvents
        {
            /// <summary>
            /// Se ejecuta cuando no hay token, el token es inválido o ha expirado (401)
            /// </summary>
            OnChallenge = async context =>
            {
                // Evita que ASP.NET Core envíe su respuesta por defecto
                context.HandleResponse();
                
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    statusCode = 401,
                    message = "No autenticado: Debe iniciar sesión para acceder a este recurso.",
                    error = "Unauthorized"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            },

            /// <summary>
            /// Se ejecuta cuando el token es válido pero el usuario no tiene el rol requerido (403)
            /// </summary>
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