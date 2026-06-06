namespace TSQR.ToolLibrary.Domain.Aggregates.MaintenanceAggregate;

public class MaintenanceRecordId(int value) : ValueObject
{
    public int Value { get; } = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
