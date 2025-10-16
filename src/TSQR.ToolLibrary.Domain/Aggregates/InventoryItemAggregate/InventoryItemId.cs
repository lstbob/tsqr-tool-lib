namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

public class InventoryItemId(int value) : ValueObject
{
    public int Value { get; } = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

