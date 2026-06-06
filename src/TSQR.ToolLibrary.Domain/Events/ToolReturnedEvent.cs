namespace TSQR.ToolLibrary.Domain.Events;

public record ToolReturnedEvent(InventoryItemId ItemId, MemberId ReturnedByMemberId, Condition ReturnedCondition) : IDomainEvent;
