using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Election.CRM.API.Common.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Previene que el navegador interprete archivos como otro tipo MIME
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Previene clickjacking (no permite que el sitio se incruste en iframes)
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Activa el filtro XSS del navegador
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Controla qué información se envía en el header Referer
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Controla qué APIs del navegador pueden usarse
        context.Response.Headers.Append("Permissions-Policy", 
            "camera=(), microphone=(), geolocation=(), payment=()");

        // Política de seguridad de contenido (CSP)
        // Para una API REST, bloqueamos todo excepto lo necesario
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'none'; frame-ancestors 'none'");

        // Fuerza el uso de HTTPS (HSTS) - solo en producción
        if (!context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            context.Response.Headers.Append("Strict-Transport-Security", 
                "max-age=31536000; includeSubDomains");
        }

        // Elimina el header Server para no revelar información del servidor
        context.Response.Headers.Remove("Server");

        await _next(context);
    }
}