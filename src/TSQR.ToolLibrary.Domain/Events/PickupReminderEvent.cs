using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Domain.Events;

public record PickupReminderEvent(ReservationId ReservationId, InventoryItemId ItemId, MemberId MemberId, DateTime PickupDate) : IDomainEvent;
