using Shop.Application.Options;
using Shop.Domain.RefreshTokens;

namespace Shop.Api.BackgroundJobs;

public sealed class RefreshTokenCleanupService(
    RefreshTokenOptions refreshTokenOptions,
    TimeProvider timeProvider,
    IServiceScopeFactory scopeFactory,
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        try
        {
            do
            {
                await RunCleanupAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // штатная остановка
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();

            var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

            var threshold = timeProvider.GetUtcNow()
                .AddDays(-refreshTokenOptions.RetentionDaysAfterExpiry);

            var removed = await refreshTokenRepository.DeleteExpiredAsync(threshold, cancellationToken);

            if (removed > 0)
                logger.LogInformation("Removed {Count} expired refresh tokens.", removed);
        }
        catch (OperationCanceledException)
        {
            // приложение останавливается — это не ошибка
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Refresh token cleanup failed.");
        }
    }
}