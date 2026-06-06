namespace TSQR.ToolLibrary.Domain.Events;

public record ToolRepairedEvent(InventoryItemId ItemId, MemberId RepairedByMemberId, Condition NewCondition) : IDomainEvent;
