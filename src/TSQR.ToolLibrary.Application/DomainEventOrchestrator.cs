using Microsoft.Extensions.Logging;

namespace TSQR.ToolLibrary.Application;

/// <summary>
/// Orchestrates in-transaction dispatch of domain events with the unit-of-work
/// commit. Domain events are dispatched to their handlers BEFORE the database
/// transaction commits so that cross-aggregate side effects roll back together
/// with the originating command. This is the eShop "Option A" pattern:
/// <see href="https://github.com/dotnet/eShop/blob/main/src/Ordering.Infrastructure/OrderingContext.cs">
/// OrderingContext.SaveEntitiesAsync</see>.
/// </summary>
/// <remarks>
/// Events raised by handlers are drained in additional passes, so handlers no
/// longer need to call the dispatcher themselves. Post-commit handlers run
/// after the commit and outside the transaction for slow, best-effort work.
/// </remarks>
public sealed class DomainEventOrchestrator(
    IUnitOfWork unitOfWork,
    IDomainEventDispatcher dispatcher,
    IDomainEventCollector collector,
    ILogger<DomainEventOrchestrator> logger
)
{
    /// <summary>
    /// Upper bound for cascaded dispatch passes. Prevents an accidental
    /// event cycle from spinning forever.
    /// </summary>
    private const int MaxDispatchDepth = 16;

    /// <summary>
    /// Dispatches the domain events collected on <paramref name="trackedAggregates"/>
    /// and any aggregates tracked by repositories during handler execution,
    /// then commits the underlying database transaction. On success, clears
    /// the events on every tracked aggregate. If a handler or the commit
    /// throws, nothing is cleared - the caller may inspect the aggregates'
    /// <see cref="Entity.DomainEvents"/> for retry / reporting.
    /// </summary>
    public async Task SaveEntitiesAsync(
        IReadOnlyCollection<Entity> trackedAggregates,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(trackedAggregates);

        var aggregates = new HashSet<Entity>(ReferenceEqualityComparer.Instance);
        foreach (var aggregate in trackedAggregates)
            aggregates.Add(aggregate);

        var dispatchedEvents = new HashSet<IDomainEvent>(ReferenceEqualityComparer.Instance);

        for (var depth = 0; ; depth++)
        {
            // Handlers can load and update aggregates that were not part of the
            // command's explicit root. Repositories track those aggregates.
            foreach (var aggregate in collector.TrackedAggregates)
                aggregates.Add(aggregate);

            var pendingEvents = new List<IDomainEvent>();
            foreach (var aggregate in aggregates)
            {
                foreach (var domainEvent in aggregate.DomainEvents)
                {
                    if (dispatchedEvents.Add(domainEvent))
                        pendingEvents.Add(domainEvent);
                }
            }

            if (pendingEvents.Count == 0)
                break;

            if (depth >= MaxDispatchDepth)
            {
                throw new InvalidOperationException(
                    $"Domain event dispatch exceeded {MaxDispatchDepth} cascaded passes. " +
                    "Check for an event cycle or a handler that keeps raising events.");
            }

            await dispatcher.DispatchAsync(pendingEvents, cancellationToken);
        }

        // Commit happens AFTER dispatch. A handler throw propagates here and
        // the underlying IUnitOfWork rolls back on dispose, so no aggregate
        // state - original or side-effect - is persisted. Atomic.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Only reached on success. Clearing events on every tracked aggregate
        // means a follow-up retry won't re-dispatch already-handled events.
        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();

        collector.Clear();

        if (dispatchedEvents.Count == 0)
            return;

        // Post-commit handlers are outside the transaction. They are
        // best-effort and idempotent; a failure is logged instead of making a
        // committed command look like it failed.
        try
        {
            await dispatcher.DispatchPostCommitAsync(
                dispatchedEvents.ToList(), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Post-commit domain event dispatch failed.");
        }
    }

    /// <summary>Convenience overload for the common single-aggregate command.</summary>
    public Task SaveEntitiesAsync(
        Entity trackedAggregate,
        CancellationToken cancellationToken = default
    ) => SaveEntitiesAsync([trackedAggregate], cancellationToken);
}
