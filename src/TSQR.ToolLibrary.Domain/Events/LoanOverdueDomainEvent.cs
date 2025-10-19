namespace TSQR.ToolLibrary.Domain.Events;

///<summary>
/// Represents an event that is raised when a loan is overdue.
/// </summary>
public record LoanOverdueDomainEvent(
    LoanId LoanId, InventoryItemId ItemId, TimeSpan OverdueTime) : IDomainEvent;

