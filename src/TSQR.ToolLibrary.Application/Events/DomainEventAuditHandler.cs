using Microsoft.Extensions.Logging;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// Audit consumer for domain events that have no downstream projection yet.
/// Recording the event is intentionally side-effect free: it keeps the
/// dispatch pipeline honest (no event is silently dropped) while the eventual
/// consumers - catalog indexing, member ledgers, notification fan-out - are
/// built out behind the same event contract.
/// </summary>
public sealed class DomainEventAuditHandler<TEvent>(
    ILogger<DomainEventAuditHandler<TEvent>> logger)
    : IDomainEventHandler<TEvent>
    where TEvent : DomainEvent
{
    public ValueTask HandleAsync(TEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Domain event {DomainEventType} handled: {DomainEvent}",
            typeof(TEvent).Name,
            domainEvent);

        return ValueTask.CompletedTask;
    }
}
