using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Domain.Events;

public record NextInLineNotificationEvent(ReservationId ReservationId, InventoryItemId ItemId, MemberId NextMemberId, string Reason) : IDomainEvent;
