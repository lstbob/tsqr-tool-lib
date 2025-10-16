namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Represents an entity in the domain-driven design context.
/// </summary>
public abstract class Entity<TId>(TId id) where TId : notnull, ValueObject 
{
   TId Id  => id;

   /// <summary>
   /// Determines whether the specified object is equal to the current entity.
   /// </summary>
   public override bool Equals(object? obj)
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

