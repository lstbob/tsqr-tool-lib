namespace TSQR.ToolLibrary.Domain.Events;

public record ReservationCreatedDomainEvent(
    ReservationId ReservationId,
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate) : IDomainEvent;
