using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryItemAggregate;

/// <summary>
/// Represents an inventory item in the tool library system.
/// </summary>
public class InventoryItem : Entity<InventoryItemId>
{
 
    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryItem"/> class.
    /// </summary>
    private InventoryItem(
            InventoryItemId id,
            MemberId originalOwnerId,
            DateTime initialAcquisitionDate,
            bool isAvailable = true,
            AmortizationRate amortizationRate,
            MemberId? currentHolderId = null,
            DateTime? lastBorrowedDate = null,
            DateTime? reservationDate = null,
            MemberId? reservationMember = null,
            ) : base(id)
    {

    }

    public MemberId OriginalOwnerId { get; }
    public DateTime InitialAcquisitionDate { get; }
    public bool IsAvailable { get;private set; }
    public AmortizationRate AmortizationRate { get; private set; }
    public MemberId? CurrentHolderId { get; private set; }
    public DateTime? LastBorrowedDate { get; private set; }
    public DateTime? ReservationDate { get; private set; }    
    public MemberId? ReservationMember {get; private set;}

    public InventoryItem Register()
    {
    }

    /// <summary>
    /// Marks the tool as lost.
    /// </summary>
    public void MarkAsLost()
    {
        if (IsAvailable)
            throw new InvalidOperationException("Tool is already available.");

        CurrentHolderId = null;
        IsAvailable = false;
    }

    /// <summary>
    /// Returns the tool to the library.
    /// </summary>
    public void Return()
    {
        if (IsAvailable)
            throw new InvalidOperationException("Tool is already available.");

        CurrentHolderId = null;
        IsAvailable = true;
    }

    /// <summary>
    /// Borrows the tool to a member.
    /// </summary>
    public void Borrow(MemberId borrower, DateTime borrowDate)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Tool is not available for borrowing.");

        ArgumentNullException.ThrowIfNull(borrower);

        if (borrowDate == default)
            throw new ArgumentNullException(nameof(borrowDate));

        if (ReservationDate is not null && ReservationDate != borrowDate)
            throw new ArgumentException("Tool");

        CurrentHolderId = borrower;
        IsAvailable = false;
        LastBorrowedDate = borrowDate;
        ReservationDate = null;
    }

    /// <summary>
    /// Reserves the tool for a member on a specific date.
    /// </summary>  
    public void Reserve(DateTime reserveDate, MemberId borrower)
    {
        if(ReservationDate is not null)
            throw new InvalidOperationException("Tool is already reserved.");

        ArgumentNullException.ThrowIfNull(borrower);

        if (reserveDate == default || reserveDate <= DateTime.UtcNow)
            throw new ArgumentNullException(nameof(reserveDate));

        // TODO: Change when loan policy is introduced
        if(reserveDate > DateTime.UtcNow.AddYears(1)) 
            throw new InvalidOperationException("Tool cannot be reserved more than a year in advance.");

        ReservationDate = reserveDate;
    }

}

