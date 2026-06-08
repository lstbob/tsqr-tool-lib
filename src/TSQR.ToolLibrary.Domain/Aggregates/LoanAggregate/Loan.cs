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
        MemberId = memberId;
        CheckoutDate = checkoutDate;
        DueDate = dueDate;
        ItemId = itemId;
        Status = status;
    }

    public MemberId MemberId { get; }
    public DateTime CheckoutDate { get; }
    public DateTime DueDate { get; }
    public InventoryItemId ItemId { get; }
    public LoanStatus Status { get; private set; }
    public DateTime ReturnedDate { get; private set; }
    public decimal FineAccrued { get; private set; }

    public static Result<Loan> Create(
        LoanId id,
        MemberId memberId,
        DateTime checkoutDate,
        DateTime dueDate,
        InventoryItemId itemId,
        LoanStatus status)
    {
        if (memberId is null)
            return new ValidationError(nameof(memberId), "Member ID is required.");
        if (itemId is null)
            return new ValidationError(nameof(itemId), "Item ID is required.");

        var checkoutResult = checkoutDate.Validate(nameof(checkoutDate));
        if (checkoutResult.IsFailure)
            return checkoutResult.Error;

        var notInFutureResult = checkoutDate.ValidateNotInFuture(nameof(checkoutDate));
        if (notInFutureResult.IsFailure)
            return notInFutureResult.Error;

        var dueDateResult = dueDate.Validate(nameof(dueDate));
        if (dueDateResult.IsFailure)
            return dueDateResult.Error;

        var notInPastResult = dueDate.ValidateNotInPast(nameof(dueDate));
        if (notInPastResult.IsFailure)
            return notInPastResult.Error;

        var statusResult = status.ValidateDefined(nameof(status));
        if (statusResult.IsFailure)
            return statusResult.Error;

        var notDefaultResult = status.ValidateNotDefault(nameof(status));
        if (notDefaultResult.IsFailure)
            return notDefaultResult.Error;

        return new Loan(id, memberId, checkoutDate, dueDate, itemId, status);
    }

    public static Result<Loan> Create(
        MemberId memberId,
        DateTime checkoutDate,
        LoanStatus status,
        DateTime dueDate,
        InventoryItemId itemId)
    {
        return Create(new LoanId(default), memberId, checkoutDate, dueDate, itemId, status);
    }

    public Result EndLoan(DateTime expectedEndDate)
    {
        var endDateResult = expectedEndDate.Validate(nameof(expectedEndDate));
        if (endDateResult.IsFailure)
            return endDateResult.Error;

        var notInPastResult = expectedEndDate.ValidateNotInPast(nameof(expectedEndDate));
        if (notInPastResult.IsFailure)
            return notInPastResult.Error;

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

        return Result.Success();
    }

    private decimal CalculateFine(TimeSpan overduePeriod)
    {
        var daysOverdue = (int)Math.Ceiling(overduePeriod.TotalDays);
        return daysOverdue * 1.00m;
    }
}
