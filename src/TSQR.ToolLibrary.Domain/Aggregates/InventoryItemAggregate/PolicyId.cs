namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents the unique identifier for a policy.
/// </summary>
public class PolicyId(int value) : ValueObject
{
    /// <summary>
    /// Gets the value of the policy identifier.
    /// </summary>
    public int Value => value; 

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

