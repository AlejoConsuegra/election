using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Election.CRM.API.Common.Extensions;

public static class KestrelExtensions
{
    public static WebApplicationBuilder AddKestrelConfiguration(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            // Eliminar header "Server" para no revelar información del servidor
            serverOptions.AddServerHeader = false;

            // Límites de concurrencia
            serverOptions.Limits.MaxConcurrentConnections = 1000;
            serverOptions.Limits.MaxConcurrentUpgradedConnections = 1000;
            serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
        });

        // Optimizar ThreadPool para mejorar rendimiento bajo carga
        ThreadPool.SetMinThreads(workerThreads: 100, completionPortThreads: 100);

        return builder;
    }
}