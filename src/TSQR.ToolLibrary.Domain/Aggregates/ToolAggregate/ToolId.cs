namespace TSQR.ToolLibrary.Domain;

public class ToolId(int value) : ValueObject
{
    public int Value { get; } = value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

