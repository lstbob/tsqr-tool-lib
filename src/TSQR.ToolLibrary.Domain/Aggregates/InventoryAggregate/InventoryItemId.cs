namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;

/// <summary>
/// Represents the unique identifier for an inventory item.
/// </summary>
public class InventoryItemId(int value) : ValueObject
{
    /// <summary>
    /// Gets the value of the inventory item identifier.
    /// </summary>
    public int Value { get; } = value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}
