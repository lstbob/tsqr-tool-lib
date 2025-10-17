namespace TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;

/// <summary>
/// Represents the unique identifier for a location in the tool library system.
/// </summary>
public class LocationId(int value) : ValueObject
{
    /// <summary>
    /// Gets the integer value of the location identifier.
    /// </summary>
    public int Value { get; } = value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

