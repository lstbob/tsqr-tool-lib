namespace TSQR.ToolLibrary.Domain.Events;

public record ReturnReminderEvent(LoanId LoanId, InventoryItemId ItemId, MemberId BorrowerId, DateTime DueDate) : IDomainEvent;
