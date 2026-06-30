namespace TSQR.ToolLibrary.Domain.Events;

public record ReservationConfirmedEvent(
    ReservationId ReservationId,
    InventoryItemId ItemId,
    MemberId MemberId
) : IDomainEvent;
