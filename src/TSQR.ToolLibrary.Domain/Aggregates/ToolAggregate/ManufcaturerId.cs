namespace TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

/// <summary>
/// Represents a manufacturer identifier in the tool library system.
/// </summary>
public class ManufcaturerId(int value) : ValueObject
{
    /// <summary>
    /// Gets the value of the manufacturer identifier.
    /// </summary>
    public int Value => value; 

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

