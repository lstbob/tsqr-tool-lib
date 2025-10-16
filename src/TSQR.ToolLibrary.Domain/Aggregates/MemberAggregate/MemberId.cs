namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

public class MemberId(int value) : ValueObject
{
    public int Value { get; } = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

