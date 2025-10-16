namespace TSQR.ToolLibrary.Domain.Entities;

/// <summary>
/// Represents an entity in the domain-driven design context.
/// </summary>
public abstract class Entity<TId> where TId : notnull, ValueObject
{
   TId Id { get; protected set; }

   /// <summary>
   /// Determines whether the specified object is equal to the current entity.
   /// </summary>
   public override Equals(object? obj)
   {
        if (obj is not Entity<TId> other)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
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
   public static bool operator  == (this Entity<TId> left, Entity<TId> right)
   {
        return Equals();
   }

   /// <summary>
   /// Inequality operator for entities.
   /// </summary>
   public static bool operator !=(this Entity<TId> left, Entity<TId> right)
   {
        return !Equals();
   }
}

