namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

public class ReservationId(int value) : ValueObject
{
    public int Value { get; } = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
