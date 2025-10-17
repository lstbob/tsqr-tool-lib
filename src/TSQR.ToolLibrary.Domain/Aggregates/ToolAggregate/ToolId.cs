namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Represents the unique identifier for a tool.
/// </summary>
public class ToolId(int value) : ValueObject
{
    public int Value { get; } = value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    } 
}

