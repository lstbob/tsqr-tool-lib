namespace TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;


/// <summary>
/// Represents an address in the tool library system.
/// </summary>
public class Address(string value) : ValueObject
{
    public string Value  {get;} = value.Validate(nameof(value)); 

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

