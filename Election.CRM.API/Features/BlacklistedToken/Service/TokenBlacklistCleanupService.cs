using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Election.CRM.API.Features.BlacklistedToken.Services;

public class TokenBlacklistCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenBlacklistCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public TokenBlacklistCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TokenBlacklistCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var blacklistService = scope.ServiceProvider.GetRequiredService<ITokenBlacklistService>();
                
                var purged = await blacklistService.PurgeExpiredAsync();
                if (purged > 0)
                {
                    _logger.LogInformation("Purga de blacklist ejecutada: {Count} tokens expirados eliminados.", purged);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la purga de la blacklist de tokens.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}