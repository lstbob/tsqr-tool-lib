using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Domain.Events;

public record ToolReservedEvent(ReservationId ReservationId, InventoryItemId ItemId, MemberId MemberId, DateTime ReservationDate) : IDomainEvent;
