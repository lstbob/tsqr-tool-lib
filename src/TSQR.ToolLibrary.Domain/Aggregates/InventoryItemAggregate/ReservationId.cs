namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents the unique identifier for a reservation.
/// </summary>
public class ReservationId(int value) : ValueObject
{
    /// <summary>
    /// Gets the value of the reservation identifier.
    /// </summary>
    public int Value { get; } = value;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
