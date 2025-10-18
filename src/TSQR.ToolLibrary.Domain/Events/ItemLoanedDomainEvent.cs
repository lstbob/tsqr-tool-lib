namespace TSQR.ToolLibrary.Domain.Events;

/// <summary>
/// Representing event triggered when an item is loaned out.
/// </summary>
public record ItemLoanedDomainEvent(
     InventoryItemId ItemId, MemberId BorrowerId, DateTime LoanDate) : IDomainEvent;

