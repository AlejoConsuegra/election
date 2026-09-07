using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Election.CRM.API.Common.Extensions;

public static class ApiBehaviorExtensions
{
    /// <summary>
    /// Configura el comportamiento de validación del modelo para devolver respuestas limpias y seguras (OWASP).
    /// </summary>
    public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                // 1. OBTENER EL LOGGER PARA REGISTRAR EL ERROR INTERNAMENTE
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                
                // 2. EXTRAER LOS ERRORES DE FORMA LIMPIA
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors?.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key!,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray()
                    );

                // 3. REGISTRAR EN CONSOLA/ARCHIVOS (Solo tú lo ves en los logs)
                logger.LogWarning("Error de validación en {Path}: {@Errors}", context.HttpContext.Request.Path, errors);

                // 4. CREAR LA RESPUESTA SEGURA PARA EL CLIENTE (Sin traceId, sin RFC, sin stack trace)
                var response = new
                {
                    statusCode = 400,
                    message = "Datos inválidos. Verifique los campos e inténtelo de nuevo.",
                    errors = errors
                };

                // 5. DEVOLVER EL RESULTADO HTTP 400 BAD REQUEST
                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}