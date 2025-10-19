namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;

/// <summary>
/// Represents the unique identifier for a loan.
/// </summary>
public class LoanId(int value) : ValueObject
{
    public int Value { get; } = value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
