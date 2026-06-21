using TSQR.ToolLibrary.Domain;

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
/// This service depends only on the technology-agnostic
/// <see cref="IUnitOfWork"/> and <see cref="IDomainEventDispatcher"/> contracts
/// from the Domain layer. A persistence adapter swap (Postgres &lt;-&gt; Mongo,
/// Dapper &lt;-&gt; EF Core) does not require any change here - the adapter
/// only has to implement <see cref="IUnitOfWork"/>.
/// </remarks>
public sealed class DomainEventOrchestrator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _dispatcher;

    public DomainEventOrchestrator(IUnitOfWork unitOfWork, IDomainEventDispatcher dispatcher)
    {
        _unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Dispatches the domain events collected on <paramref name="trackedAggregates"/>
    /// to their handlers, then commits the underlying database transaction. On
    /// success, clears the events on every tracked aggregate. If a handler or
    /// the commit throws, nothing is cleared - the caller may inspect the
    /// aggregates' <see cref="Entity.DomainEvents"/> for retry / reporting.
    /// </summary>
    public async Task SaveEntitiesAsync(IReadOnlyCollection<Entity> trackedAggregates, CancellationToken cancellationToken = default)
    {
        var allEvents = trackedAggregates.SelectMany(a => a.DomainEvents).ToList();

        if (allEvents.Count > 0)
            await _dispatcher.DispatchAsync(allEvents, cancellationToken);

        // Commit happens AFTER dispatch. A handler throw propagates here and
        // the underlying IUnitOfWork rolls back on dispose, so no aggregate
        // state - original or side-effect - is persisted. Atomic.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Only reached on success. Clearing events on every tracked aggregate
        // means a follow-up retry won't re-dispatch already-handled events.
        foreach (var aggregate in trackedAggregates)
            aggregate.ClearDomainEvents();
    }

    /// <summary>Convenience overload for the common single-aggregate command.</summary>
    public Task SaveEntitiesAsync(Entity trackedAggregate, CancellationToken cancellationToken = default)
        => SaveEntitiesAsync([trackedAggregate], cancellationToken);
}