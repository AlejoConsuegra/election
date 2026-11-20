using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Election.CRM.API.Common.DTOs; // <-- Importante importar esto

namespace Election.CRM.API.Common.Extensions;

public static class ApiBehaviorExtensions
{
    public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = false;

            options.InvalidModelStateResponseFactory = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                // 1. Extraer los errores de forma limpia (Diccionario: Campo -> Lista de mensajes)
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors?.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key!,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray()
                    );

                logger.LogWarning("Error de validación en {Path}: {@Errors}", context.HttpContext.Request.Path, errors);

                // 2. ¡AQUÍ ESTÁ LA UNIFICACIÓN! Usamos ApiResponse.BuildError
                var response = ApiResponse<object>.BuildError(
                    "Datos inválidos. Verifique los campos e inténtelo de nuevo.",
                    400,
                    errors // Pasamos el diccionario al campo 'Extra'
                );

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}