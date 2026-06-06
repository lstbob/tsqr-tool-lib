namespace TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;

public class Loan : Entity<LoanId>, IAggregateRoot
{
    private Loan(
        LoanId id,
        MemberId memberId,
        DateTime checkoutDate,
        DateTime dueDate,
        InventoryItemId itemId,
        LoanStatus status) : base(id)
    {
        MemberId = memberId ?? throw new ArgumentNullException(nameof(memberId));
        CheckoutDate = checkoutDate.Validate(nameof(checkoutDate)).ValidateNotInFuture(nameof(checkoutDate));
        DueDate = dueDate.Validate(nameof(dueDate)).ValidateNotInPast(nameof(dueDate));
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        Status = status.ValidateDefined(nameof(status)).ValidateNotDefault(nameof(status));
    }

    public MemberId MemberId { get; }
    public DateTime CheckoutDate { get; }
    public DateTime DueDate { get; }
    public InventoryItemId ItemId { get; }
    public LoanStatus Status { get; private set; }
    public DateTime ReturnedDate { get; private set; }
    public decimal FineAccrued { get; private set; }

    public static Loan Create(
        LoanId id,
        MemberId memberId,
        DateTime checkoutDate,
        DateTime dueDate,
        InventoryItemId itemId,
        LoanStatus status)
    {
        return new(id, memberId, checkoutDate, dueDate, itemId, status);
    }

    public static Loan Create(
        MemberId memberId,
        DateTime checkoutDate,
        LoanStatus status,
        DateTime dueDate,
        InventoryItemId itemId)
    {
        return new(new LoanId(default), memberId, checkoutDate, dueDate, itemId, status);
    }

    public void EndLoan(DateTime expectedEndDate)
    {
        _ = expectedEndDate.Validate(nameof(expectedEndDate)).ValidateNotInPast(nameof(expectedEndDate));

        if (expectedEndDate > DueDate)
        {
            Status = LoanStatus.Overdue;
            TimeSpan overdueTime = expectedEndDate - DueDate;
            FineAccrued = CalculateFine(overdueTime);
            AddDomainEvent(new LoanOverdueDomainEvent(Id, ItemId, overdueTime));
        }
        else
        {
            Status = LoanStatus.Returned;
            ReturnedDate = DateTime.UtcNow;
        }
    }

    private decimal CalculateFine(TimeSpan overduePeriod)
    {
        var daysOverdue = (int)Math.Ceiling(overduePeriod.TotalDays);
        return daysOverdue * 1.00m;
    }
}
