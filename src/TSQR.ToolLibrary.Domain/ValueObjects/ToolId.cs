namespace TSQR.ToolLibrary.Domain.ValueObjects;

public record ToolId(int Value) : ValueObject
{
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

