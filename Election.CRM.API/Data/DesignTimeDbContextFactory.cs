using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Election.CRM.API.Data;

namespace Election.CRM.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CrmContext>
{
    public CrmContext CreateDbContext(string[] args)
    {
        // Ruta al proyecto donde están los appsettings
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var builder = new DbContextOptionsBuilder<CrmContext>();
        
        // Obtiene la cadena de conexión de los archivos JSON
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseMySql(
            connectionString,
            ServerVersion.Parse("8.0.46")
        );

        return new CrmContext(builder.Options);
    }
}