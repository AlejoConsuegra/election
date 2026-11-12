using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Election.CRM.API.Common.Services;

public class CookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;

    public CookieService(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
    {
        _httpContextAccessor = httpContextAccessor;
        _env = env;
    }

   public void SetAuthCookies(string accessToken, string refreshToken, DateTime expiration)
{
    var context = _httpContextAccessor.HttpContext;
    if (context == null) return;

    var isDev = _env.IsDevelopment();
    
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        // ⚠️ En desarrollo LOCAL sin HTTPS, debes forzar Secure = false.
        // Si usas HTTPS local (ej. la url de IISExpress/Kestrel con https://localhost:5001), déjalo en true.
        Secure = false, 
        
        // ⚠️ Para desarrollo con puertos distintos, pon SameSiteMode.Unspecified.
        // Esto le dice al navegador que use el comportamiento por defecto antiguo y no bloquee el puerto cruzado en HTTP.
        SameSite = isDev ? SameSiteMode.Unspecified : SameSiteMode.None, 
        
        Expires = expiration,
        Path = "/"
    };
    
    // Cambia los nombres aquí para que coincidan con tu Middleware si lo requieres,
    // o ajusta tu middleware para leer "accessToken" en lugar de "auth_token".
    context.Response.Cookies.Append("auth_token", accessToken, cookieOptions); // Cambiado a auth_token
    context.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
}
    public void ClearAuthCookies()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var isDev = _env.IsDevelopment();

        // Las opciones DEBEN ser idénticas a las de SetAuthCookies para que el navegador permita el borrado.
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Debe coincidir exactamente con SetAuthCookies en desarrollo local
            SameSite = isDev ? SameSiteMode.Unspecified : SameSiteMode.None, // Debe coincidir exactamente
            Path = "/", // CRÍTICO: Sin esto, el navegador no encuentra la cookie raíz para borrarla
            Expires = DateTime.UtcNow.AddDays(-1) // Forzar expiración inmediata en el pasado
        };

        context.Response.Cookies.Delete("auth_token", cookieOptions);
        context.Response.Cookies.Delete("refreshToken", cookieOptions);
    }
}