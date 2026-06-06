namespace TSQR.ToolLibrary.Domain.Events;

public record ToolMarkedForRepairEvent(InventoryItemId ItemId, MemberId ReportedByMemberId, string Description) : IDomainEvent;
