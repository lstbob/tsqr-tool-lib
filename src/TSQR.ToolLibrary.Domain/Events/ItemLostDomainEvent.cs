namespace TSQR.ToolLibrary.Domain.Events;

public record ItemLostDomainEvent(InventoryItemId ItemId, MemberId ReportedByMemberId, MemberId? PreviousHolderId) : DomainEvent;
