using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Election.CRM.API.Common.Extensions;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Configura el pipeline de middleware principal de la aplicación
    /// </summary>
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        // Swagger solo en desarrollo
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Pipeline de middleware en el orden correcto
        app.UseHttpsRedirection();
        app.UseCors("AllowAngular");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Elimina el header "Server" de las respuestas HTTP
    /// </summary>
    public static IApplicationBuilder UseServerHeaderRemoval(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Remove("Server");
                return Task.CompletedTask;
            });
            await next();
        });
    }
}