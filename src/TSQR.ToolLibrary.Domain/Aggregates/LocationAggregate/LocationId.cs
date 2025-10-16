namespace TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;

public class LocationId(int value) : ValueObject
{
    public int Value { get; } = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

