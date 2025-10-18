namespace TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;

/// <summary>
/// Represents a loan of an inventory item in the tool library system.
/// </summary>
public class Loan : Entity<LoanId>
{
    private Loan(
            LoanId id,
            MemberId memberId,
            DateTime checkoutDate,
            DateTime dueDate,
            InventoryItemId itemId,
            LoanStatus status
            ) : base(id)
    {
        MemberId = memberId ?? throw new ArgumentNullException(nameof(memberId));

        CheckoutDate = checkoutDate
            .Validate(nameof(checkoutDate))
            .ValidateNotInFuture(nameof(checkoutDate));

        Status = status
            .ValidateDefined(nameof(status))
            .ValidateNotDefault(nameof(status));

        DueDate = dueDate
            .Validate(nameof(dueDate))
            .ValidateNotInPast(nameof(dueDate));

        ItemId = itemId;
        Status = status;

    }

    public MemberId MemberId { get; }
    public DateTime CheckoutDate { get; }
    public DateTime DueDate { get; }
    public InventoryItemId ItemId { get; }
    public LoanStatus Status { get; private set; }
    public DateTime ReturnedDate { get; private set; }
    public decimal FineAccrued {get; private set; }
    
    /// <summary>
    /// Factory method to rehydrate an existing <see cref="Loan"/> instance with a new ID. 
    /// </summary>
    public static Loan Create(
            LoanId id, 
            MemberId memberId,
            DateTime checkoutDate,
            DateTime dueDate
            InventoryItemId itemId,
            LoanStatus status)
    {
        return new(
            id,
            memberId,
            checkoutDate,
            dueDate,
            itemId,
            status);
    }

    /// <summary>
    /// Factory method to create a <see cref="Loan"/> instance. 
    /// </summary>
    public static Loan Create(
            MemberId memberId,
            DateTime checkoutDate,
            LoanStatus status,
            DateTime dueDate,
            InventoryItemId itemId)
    {
        return new (
                new LoanId(default),
                memberId,
                checkoutDate,
                dueDate,
                itemId,
                status);
    }

    /// <summary>
    /// Responsible for ending a loan;
    /// </summary>
    public void EndLoan(DateTime expectedEndDate) 
    {
        _ = expectedEndDate
            .Validate(nameof(expectedEndDate))
            .ValidateNotInPast(nameof(expectedEndDate));

        if(expectedEndDate > DueDate )
        {
            Status = LoanStatus.Overdue;
            TimeSpan overdueTime = expectedEndDate - DueDate;
            AddDomainEvent(new LoanOverdueDomainEvent(Id, ItemId, overdueTime));
        } else
        {
            Status = LoanStatus.Returned;
            ReturnedDate = DateTime.UtcNow;
        }
    }

}

