namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Represents an entity in the domain-driven design context.
/// </summary>
public abstract class Entity<TId>(TId id) where TId : notnull, ValueObject 
{
   private List<IDomainEvent> _domainEvents = [];
   public TId Id  {get; } = id;
   public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

   /// <summary>
   /// Adds a domain event to the entity.
   /// </summary>
   public void AddDomainEvent(IDomainEvent eventItem)
   {
        _domainEvents ??= [];
        _domainEvents.Add(eventItem);
   }

   /// <summary>
   /// Removes a domain event from the entity.
   /// </summary>
   public void RemoveDomainEvent(IDomainEvent eventItem)
   {
        _domainEvents?.Remove(eventItem);
   }    

   /// <summary>
   /// Clears all domain events from the entity.
   /// </summary>
   public void ClearDomainEvents()
   {
        _domainEvents?.Clear();
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
   public static bool operator  == (Entity<TId> left, Entity<TId> right)
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

