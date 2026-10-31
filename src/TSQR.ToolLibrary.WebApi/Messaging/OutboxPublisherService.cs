using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TSQR.Common.Messaging;
using TSQR.ToolLibrary.Domain;

namespace TSQR.ToolLibrary.WebApi.Messaging;

/// <summary>
/// Background outbox publisher. Each pass runs in its own scope and commits
/// the processed/failed markers with the same unit of work that read the batch.
/// </summary>
public sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox publisher loop failed.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
        var transport = scope.ServiceProvider.GetRequiredService<IIntegrationEventTransport>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var messages = await store.DequeueAsync(BatchSize, cancellationToken);
        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                await transport.PublishAsync(message, cancellationToken);
                await store.MarkProcessedAsync(message.EventId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                await store.MarkFailedAsync(message.EventId, ex.Message, cancellationToken);
                logger.LogError(
                    ex,
                    "Failed to publish integration event {EventId} ({EventType}).",
                    message.EventId,
                    message.EventType);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
