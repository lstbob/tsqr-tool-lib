using TSQR.ToolLibrary.Domain.Services;

namespace TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;

public class Loan : Entity<LoanId>, IAggregateRoot
{
    public const decimal DefaultLateFeePerDay = 1.00m;
    public const int DefaultMaxLoanDurationDays = 7;

    private Loan(
        LoanId id,
        MemberId memberId,
        DateTime checkoutDate,
        DateTime dueDate,
        InventoryItemId itemId,
        LoanStatus status,
        decimal lateFeePerDay,
        int renewalCount,
        int communityId = 0) : base(id)
    {
        MemberId = memberId;
        CheckoutDate = checkoutDate;
        DueDate = dueDate;
        ItemId = itemId;
        Status = status;
        LateFeePerDay = lateFeePerDay;
        RenewalCount = renewalCount;
        CommunityId = communityId;
    }

    public MemberId MemberId { get; }
    public DateTime CheckoutDate { get; }
    public DateTime DueDate { get; private set; }
    public InventoryItemId ItemId { get; }
    public LoanStatus Status { get; private set; }
    public DateTime ReturnedDate { get; private set; }
    public decimal FineAccrued { get; private set; }

    /// <summary>
    /// Per-day late fee snapshotted from the <see cref="Aggregates.ToolAggregate.Policy"/>
    /// at checkout. Policy changes made after a loan has begun do not affect
    /// the rate applied to in-flight loans — the agreement is "frozen" the
    /// moment the tool leaves the shelf. This is the DDD-correct pattern for
    /// capturing policy at a domain action.
    /// </summary>
    public decimal LateFeePerDay { get; }

    /// <summary>
    /// Number of times this loan has been renewed. Bounded by the originating
    /// <see cref="Aggregates.ToolAggregate.Policy.MaxRenewalCount"/> via the
    /// <see cref="Renew"/> method.
    /// </summary>
    public int RenewalCount { get; private set; }

    public int CommunityId { get; private set; }

    public static Result<Loan> Create(
        LoanId id,
        MemberId memberId,
        DateTime checkoutDate,
        DateTime dueDate,
        InventoryItemId itemId,
        LoanStatus status,
        decimal lateFeePerDay = DefaultLateFeePerDay,
        int renewalCount = 0,
        int communityId = 0)
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

        if (lateFeePerDay < 0)
            return new ValidationError(nameof(lateFeePerDay), "Late fee per day cannot be negative.");

        if (renewalCount < 0)
            return new ValidationError(nameof(renewalCount), "Renewal count cannot be negative.");

        return new Loan(id, memberId, checkoutDate, dueDate, itemId, status, lateFeePerDay, renewalCount, communityId);
    }

    /// <summary>
    /// Backward-compatible factory. Picks a default 7-day loan duration and the
    /// legacy $1.00/day fine rate. New call sites should use the
    /// <see cref="Create(MemberId, InventoryItemId, int, decimal, int)"/> overload
    /// which derives both from the active <see cref="Aggregates.ToolAggregate.Policy"/>.
    /// </summary>
    public static Result<Loan> Create(
        MemberId memberId,
        DateTime checkoutDate,
        LoanStatus status,
        DateTime dueDate,
        InventoryItemId itemId,
        int communityId = 0)
    {
        return Create(new LoanId(default), memberId, checkoutDate, dueDate, itemId, status, DefaultLateFeePerDay, 0, communityId);
    }

    /// <summary>
    /// Policy-driven factory. <paramref name="maxLoanDurationDays"/> and
    /// <paramref name="lateFeePerDay"/> are taken from the active Policy at the
    /// moment of checkout and snapshotted on the new Loan so policy changes
    /// cannot retroactively affect in-flight loans (see <see cref="LateFeePerDay"/>).
    /// Raises <see cref="LoanCreatedDomainEvent"/> on success.
    /// </summary>
    public static Result<Loan> Create(
        MemberId memberId,
        InventoryItemId itemId,
        int maxLoanDurationDays,
        decimal lateFeePerDay,
        int communityId = 0)
    {
        if (maxLoanDurationDays <= 0)
            return new ValidationError(nameof(maxLoanDurationDays), "Max loan duration days must be positive.");

        var checkoutDate = DateTime.UtcNow;
        var dueDate = checkoutDate.AddDays(maxLoanDurationDays);
        var loan = Create(new LoanId(default), memberId, checkoutDate, dueDate, itemId, LoanStatus.Active, lateFeePerDay, 0, communityId);
        if (loan.IsSuccess)
            loan.Value.AddDomainEvent(new LoanCreatedDomainEvent(itemId, memberId, communityId));
        return loan;
    }

    /// <summary>
    /// Backward-compatible default factory. Uses the legacy 7-day duration and
    /// $1.00/day fine rate. Prefer the Policy-driven overload above in handlers
    /// that have access to the active Policy.
    /// </summary>
    public static Result<Loan> Create(
        MemberId memberId,
        InventoryItemId itemId,
        int communityId = 0)
        => Create(memberId, itemId, DefaultMaxLoanDurationDays, DefaultLateFeePerDay, communityId);

    public Result EndLoan(DateTime expectedEndDate)
    {
        if (Status != LoanStatus.Active)
            return new DomainError(nameof(Status), "Only active loans can be ended.");

        var endDateResult = expectedEndDate.Validate(nameof(expectedEndDate));
        if (endDateResult.IsFailure)
            return endDateResult.Error;

        // No not-in-past check: the loan-end moment is "now" (DateTime.UtcNow from the
        // handler), which is always microseconds behind a fresh UtcNow, so ValidateNotInPast
        // would reject every call. Ending a loan can never legitimately be a future date.
        if (expectedEndDate > DueDate)
        {
            Status = LoanStatus.Overdue;
            TimeSpan overdueTime = expectedEndDate - DueDate;
            FineAccrued = CalculateFine(expectedEndDate);
            AddDomainEvent(new LoanOverdueDomainEvent(Id, ItemId, overdueTime));
        }
        else
        {
            Status = LoanStatus.Returned;
            ReturnedDate = DateTime.UtcNow;
        }

        return Result.Success();
    }

    /// <summary>
    /// Renews the loan, extending the due date by <paramref name="maxLoanDurationDays"/>
    /// (taken from the active <see cref="Aggregates.ToolAggregate.Policy"/> at the time
    /// of renewal). Rejects renewal if the loan is not currently active, if the loan
    /// has already been returned/ended, or if the policy's
    /// <see cref="Aggregates.ToolAggregate.Policy.MaxRenewalCount"/> would be exceeded.
    /// Does not raise a domain event — the application-layer RenewLoanCommand (tracked
    /// separately) is responsible for any side effects.
    /// </summary>
    public Result Renew(int maxLoanDurationDays, int maxRenewalCount)
    {
        if (Status != LoanStatus.Active)
            return new DomainError(nameof(Status), "Only active loans can be renewed.");

        if (maxLoanDurationDays <= 0)
            return new ValidationError(nameof(maxLoanDurationDays), "Max loan duration days must be positive.");

        if (maxRenewalCount <= 0)
            return new ValidationError(nameof(maxRenewalCount), "Max renewal count must be positive.");

        // Cap renewal at < MaxRenewalCount - i.e. MaxRenewalCount=2 means at most
        // two renewals are allowed (renewal #1 and #2). The third renewal is rejected.
        if (RenewalCount >= maxRenewalCount)
            return new DomainError(nameof(RenewalCount), "Loan has reached the maximum number of renewals allowed by the policy.");

        DueDate = DueDate.AddDays(maxLoanDurationDays);
        RenewalCount++;
        return Result.Success();
    }

    private decimal CalculateFine(DateTime returnDate)
    {
        // Delegates to the domain service so fine calculation is policy-driven
        // even when invoked from inside the aggregate. The snapshot rate on
        // this loan (LateFeePerDay) is what the policy said at checkout time.
        var fineService = new FineService();
        return fineService.CalculateFine(this, returnDate, LateFeePerDay).Value;
    }
}