using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Domain.Events;

public record ReservationCancelledEvent(ReservationId ReservationId, InventoryItemId ItemId, MemberId MemberId) : IDomainEvent;
