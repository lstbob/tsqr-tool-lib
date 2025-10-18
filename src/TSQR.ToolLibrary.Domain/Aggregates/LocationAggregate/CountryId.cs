namespace TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;

/// <summary>
/// Represents the identifier for a country in the tool library system.
/// </summary>
public class CountryId(int value) : ValueObject
{
    public int Value { get; } = value.Validate(nameof(value));

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }    
}
