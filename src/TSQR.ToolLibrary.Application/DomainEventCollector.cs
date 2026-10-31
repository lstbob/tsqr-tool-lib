namespace TSQR.ToolLibrary.Application;

/// <summary>
/// Scoped collector shared by repositories and <see cref="DomainEventOrchestrator"/>.
/// Uses reference equality so two aggregates with the same identifier are not
/// collapsed.
/// </summary>
public sealed class DomainEventCollector : IDomainEventCollector
{
    private readonly HashSet<Entity> _aggregates = new(ReferenceEqualityComparer.Instance);

    public IReadOnlyCollection<Entity> TrackedAggregates => _aggregates;

    public void Track(Entity aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        _aggregates.Add(aggregate);
    }

    public void Clear() => _aggregates.Clear();
}
