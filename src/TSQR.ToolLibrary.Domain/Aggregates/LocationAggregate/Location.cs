namespace TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;

/// <summary>
/// Represents a location in the tool library system.
/// </summary>
public class Location : Entity<LocationId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Location"/> class.
    /// </summary>
    private Location(LocationId id, string name) : base(id)
    {
        Name = name.Validate(nameof(name));
    }

    public string Name { get; }    
}

