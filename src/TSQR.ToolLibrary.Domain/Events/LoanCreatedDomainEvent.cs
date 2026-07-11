namespace TSQR.ToolLibrary.Domain.Events;

public record LoanCreatedDomainEvent(InventoryItemId ItemId, MemberId MemberId, int CommunityId) : IDomainEvent;
