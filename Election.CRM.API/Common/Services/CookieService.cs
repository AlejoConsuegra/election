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
            Secure = !isDev, 
            SameSite = isDev ? SameSiteMode.Unspecified : SameSiteMode.None,             
            Expires = expiration,
            Path = "/"
        };
        
        context.Response.Cookies.Append("auth_token", accessToken, cookieOptions); 
        context.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        // Cookie usada solo para decirle a angular que la sesión está activa.
        // no agrega vulneración alguna a la seguridad.
        context.Response.Cookies.Append("session_active", "true", new CookieOptions
        {
            HttpOnly = false, // Angular DEBE poder leer esta
            Secure = !isDev,  // Seguro en producción
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
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
            Secure = !isDev, // Debe coincidir exactamente con SetAuthCookies en desarrollo local
            SameSite = isDev ? SameSiteMode.Unspecified : SameSiteMode.None, // Debe coincidir exactamente
            Path = "/", // CRÍTICO: Sin esto, el navegador no encuentra la cookie raíz para borrarla
            Expires = DateTime.UtcNow.AddDays(-1) // Forzar expiración inmediata en el pasado
        };

        context.Response.Cookies.Delete("auth_token", cookieOptions);
        context.Response.Cookies.Delete("refreshToken", cookieOptions);

        context.Response.Cookies.Delete("session_active", new CookieOptions 
        { 
            Path = "/",
            Secure = !isDev,
            SameSite = SameSiteMode.Lax
        });
    }
}