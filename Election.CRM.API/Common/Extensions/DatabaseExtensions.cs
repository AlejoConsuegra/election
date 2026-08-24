using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Data;

namespace Election.CRM.API.Common.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CrmContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb")
            ));

        return services;
    }
}