namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Non-generic base class for all domain entities. Holds the domain-event
/// collection so the application layer's <see cref="IDomainEventDispatcher"/>
/// can dispatch and clear events across aggregates without knowing their
/// identifier type. Mirrors the eShop reference
/// (<c>src/Ordering.Domain/SeedWork/Entity.cs</c>).
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// The domain events raised by this aggregate since the last dispatch.
    /// Exposed as a read-only list so the getter does not allocate a wrapper
    /// on every access.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>Records a domain event on this aggregate for later dispatch.</summary>
    public void AddDomainEvent(IDomainEvent eventItem)
    {
        ArgumentNullException.ThrowIfNull(eventItem);
        _domainEvents.Add(eventItem);
    }

    /// <summary>Removes a previously recorded domain event.</summary>
    public void RemoveDomainEvent(IDomainEvent eventItem) => _domainEvents.Remove(eventItem);

    /// <summary>
    /// Clears the collected domain events. Called by the application layer
    /// only after a successful transaction commit.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Base class for domain entities identified by a strongly-typed value-object id.
/// </summary>
/// <typeparam name="TId">The value-object identifier type.</typeparam>
public abstract class Entity<TId>(TId id) : Entity
    where TId : notnull, ValueObject
{
    private TId _id = id;

    public TId Id => _id;

    internal void SetAssignedId(TId id)
    {
        _id = id;
    }

    public Task<int> TestAsnc()
    {
        return Task.FromResult(3);
    }

    /// <summary>
    /// Determines whether the entity is transient (i.e., has not been persisted).
    /// </summary>
    public bool IsTransient()
    {
        return Id.Equals(default(TId));
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is Entity<TId>))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        Entity<TId> item = (Entity<TId>)obj;

        if (item.IsTransient() || IsTransient())
            return false;
        else
            return item.Id == Id;
    }

    /// <summary>
    /// Returns a hash code for the entity.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Equality operator for entities.
    /// </summary>
    public static bool operator ==(Entity<TId> left, Entity<TId> right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Inequality operator for entities.
    /// </summary>
    public static bool operator !=(Entity<TId> left, Entity<TId> right)
    {
        return !Equals(left, right);
    }
}

