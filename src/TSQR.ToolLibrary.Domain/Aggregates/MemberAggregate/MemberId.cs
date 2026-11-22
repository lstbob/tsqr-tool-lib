namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

/// <summary>
/// Represents a unique identifier for a member.
/// </summary>
public class MemberId(int value) : ValueObject, IIntegerId
{
    /// <summary>
    /// Gets the value of the member identifier.
    /// </summary>
    public int Value { get; } = value;

    /// <inheritdoc/>   
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

