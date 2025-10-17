using TSQR.ToolLibrary.Domain.Events;

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
            ToolId toolId,
            MemberId originalOwnerId,
            DateTime initialAcquisitionDate,
            ItemStatus status,
            Condition condition,
            MemberId? currentHolderId = null,
            DateTime? lastBorrowedDate = null,
            DateTime? reservationDate = null,
            MemberId? reservationMemberId = null
            ) : base(id)
    {
        ToolId = toolId ?? throw new ArgumentNullException(nameof(toolId));
        OriginalOwnerId = originalOwnerId ?? throw new ArgumentNullException(nameof(originalOwnerId));
        InitialAcquisitionDate = initialAcquisitionDate == default?
             throw new ArgumentNullException(nameof(initialAcquisitionDate)) 
             : initialAcquisitionDate;
        Status = status;
        Condition = condition;
        CurrentHolderId = currentHolderId;
        LastBorrowedDate = lastBorrowedDate;
        ReservationDate = reservationDate;
        ReservationMemberId = reservationMemberId;
    }

    public ToolId ToolId { get; }
    public MemberId OriginalOwnerId { get; }
    public DateTime InitialAcquisitionDate { get; }
    public ItemStatus Status { get;private set; }
    public Condition Condition { get; private set; }
    public MemberId? CurrentHolderId { get; private set; }
    public DateTime? LastBorrowedDate { get; private set; }
    public DateTime? ReservationDate { get; private set; }    
    public MemberId? ReservationMemberId {get; private set;}

    /// <summary>
    /// Factory method to create a new instance of the <see cref="InventoryItem"/> class
    /// </summary>
    public static InventoryItem Create(
            ToolId toolId,
            MemberId originalOwnerId,
            DateTime initialAcquisitionDate,
            Condition condition)
    {
        return new(
            new InventoryItemId(default),
            toolId,
            originalOwnerId,
            initialAcquisitionDate,
            ItemStatus.Available,
            condition);
    }

    /// <summary>
    /// Factory method to rehydrate an existing instance of the <see cref="InventoryItem"/> class.
    /// </summary>
    public static InventoryItem Create(
            InventoryItemId id,
            ToolId toolId,
            MemberId originalOwnerId,
            DateTime initialAcquisitionDate,
            ItemStatus status,
            Condition condition,
            MemberId currentHolderId, 
            DateTime lastBorrowedDate,
            DateTime reservationDate,
            MemberId reservationMemberId)
    {
        return new(
            id,
            toolId,
            originalOwnerId,
            initialAcquisitionDate,
            status,
            condition,
            currentHolderId,
            lastBorrowedDate,
            reservationDate,
            reservationMemberId);
    }
    
    /// <summary>
    /// Marks the tool as lost.
    /// </summary>
    public void MarkAsLost(MemberId reporter)
    {
        if (Status.Equals(ItemStatus.Lost))
            throw new InvalidOperationException("Tool is already marked as lost.");

        CurrentHolderId = null;
        Status = ItemStatus.Lost;
        // TODO: Add Domain Event for reporting lost tool. Notifying original owner and admin.
        // CurrentHolder should pay a fine or replace the tool.
        // 
    }

    /// <summary>
    /// Returns the tool to the library.
    /// </summary>
    public void Return()
    {
        if (!Status.Equals(ItemStatus.Loaned))
            throw new InvalidOperationException("Tool is not currently loaned out.");

        CurrentHolderId = null;
        Status = ItemStatus.Available;
    }

    /// <summary>
    /// Loans the tool to a member.
    /// </summary>
    public void Loan(MemberId memberId)
    {
        ArgumentNullException.ThrowIfNull(memberId);

        if (!Status.Equals(ItemStatus.Available))
            throw new InvalidOperationException("Tool is not available for borrowing.");

        if(ReservationDate is not null || ReservationMemberId is not null)
            throw new InvalidOperationException("Tool is reserved and cannot be borrowed.");

        CurrentHolderId = memberId;
        Status = ItemStatus.Loaned;
        LastBorrowedDate = DateTime.UtcNow;
        AddDomainEvent(new ItemLoanedEvent(Id, memberId, LastBorrowedDate.Value));
    }

    /// <summary>
    /// Reserves the tool for a member on a specific date.
    /// </summary>  
    public void Reserve(DateTime reserveDate, MemberId member)
    {
        if(ReservationDate is not null)
            throw new InvalidOperationException("Tool is already reserved.");

        ArgumentNullException.ThrowIfNull(member);

        if (reserveDate == default || reserveDate <= DateTime.UtcNow)
            throw new ArgumentNullException(nameof(reserveDate));

        // TODO: Change when loan policy is introduced
        if(reserveDate > DateTime.UtcNow.AddYears(1)) 
            throw new InvalidOperationException("Tool cannot be reserved more than a year in advance.");

        ReservationDate = reserveDate;
    }

}

