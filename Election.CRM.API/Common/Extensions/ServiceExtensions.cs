using Election.CRM.API.Common.Services;

using Election.CRM.API.Features.Alertas;
using Election.CRM.API.Features.AsignacionZona;
using Election.CRM.API.Features.NombreZona;
using Election.CRM.API.Features.Ocasionale;
using Election.CRM.API.Features.Registro;
using Election.CRM.API.Features.Role;
using Election.CRM.API.Features.User;
using Election.CRM.API.Features.Votante;
using Election.CRM.API.Features.Auth;

using Election.CRM.API.Features.BlacklistedToken.Services;

using Election.CRM.API.Features.AsignacionZona.Validators;
using Election.CRM.API.Features.NombreZona.Validators;
using Election.CRM.API.Features.Ocasionale.Validators;
using Election.CRM.API.Features.Registro.Validators;
using Election.CRM.API.Features.Role.Validators;
using Election.CRM.API.Features.User.Validators; 
using Election.CRM.API.Features.Votante.Validators;

namespace Election.CRM.API.Common.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<LoginAttemptService>();
        services.AddSingleton<RefreshTokenService>();
        services.AddScoped<ExcelService>();
        
        services.AddScoped<JwtService>(); 
        services.AddScoped<CurrentUserService>();  
        services.AddScoped<CookieService>();

        services.AddScoped<AlertaService>();
        services.AddScoped<AsignacionZonaService>();
        services.AddScoped<NombreZonaService>();
        services.AddScoped<OcasionaleValidator>();
        services.AddScoped<RegistroService>();
        services.AddScoped<RoleService>();
        services.AddScoped<UserService>();
        services.AddScoped<VotanteService>();
        services.AddScoped<AuthService>();
        

        services.AddScoped<AsignacionZonaValidator>();
        services.AddScoped<NombreZonaValidator>();
        services.AddScoped<OcasionaleService>();
        services.AddScoped<RegistroValidator>();
        services.AddScoped<RoleValidator>();
        services.AddScoped<UserValidator>();   
        services.AddScoped<VotanteValidator>();

        services.AddScoped<RegistroExcelProcessor>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
        services.AddHostedService<TokenBlacklistCleanupService>();
        
        return services;
    }
}