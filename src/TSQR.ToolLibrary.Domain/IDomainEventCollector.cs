namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Tracks aggregates touched during the current unit of work. Repositories
/// register aggregates on add/update so the orchestrator can drain events that
/// handlers raise on aggregates outside the command's explicit root.
/// </summary>
public interface IDomainEventCollector
{
    IReadOnlyCollection<Entity> TrackedAggregates { get; }

    void Track(Entity aggregate);

    void Clear();
}
