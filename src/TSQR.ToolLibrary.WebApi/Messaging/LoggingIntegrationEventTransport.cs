using Microsoft.Extensions.Logging;
using TSQR.Common.Messaging;

namespace TSQR.ToolLibrary.WebApi.Messaging;

/// <summary>
/// Placeholder transport that records outbox deliveries in the application log.
/// Replace this with the real HTTP client / broker producer when a cross-domain
/// transport is selected.
/// </summary>
public sealed class LoggingIntegrationEventTransport(
    ILogger<LoggingIntegrationEventTransport> logger) : IIntegrationEventTransport
{
    public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Published integration event {EventId} ({EventType}) from the outbox.",
            message.EventId,
            message.EventType);

        return Task.CompletedTask;
    }
}
