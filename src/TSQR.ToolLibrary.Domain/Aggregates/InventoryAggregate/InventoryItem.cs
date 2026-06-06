namespace TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;

public class InventoryItem : Entity<InventoryItemId>, IAggregateRoot
{
    private InventoryItem(
        InventoryItemId id,
        ToolId toolId,
        MemberId originalOwnerId,
        DateTime initialAcquisitionDate,
        string serialNumber,
        ItemStatus status,
        Condition condition,
        MemberId? currentHolderId = null,
        DateTime? lastBorrowedDate = null,
        DateTime? reservationDate = null,
        MemberId? reservationMemberId = null,
        int loanCount = 0,
        TimeSpan totalUsageTime = default,
        bool isUnderRepair = false) : base(id)
    {
        ToolId = toolId ?? throw new ArgumentNullException(nameof(toolId));
        OriginalOwnerId = originalOwnerId ?? throw new ArgumentNullException(nameof(originalOwnerId));
        InitialAcquisitionDate = initialAcquisitionDate.Validate(nameof(initialAcquisitionDate)).ValidateNotInFuture(nameof(initialAcquisitionDate));
        SerialNumber = serialNumber.Validate(serialNumber);
        Status = status.ValidateDefined(nameof(status)).ValidateNotDefault(nameof(status));
        Condition = condition.ValidateDefined(nameof(condition)).ValidateNotDefault(nameof(condition));
        CurrentHolderId = currentHolderId;
        LastBorrowedDate = lastBorrowedDate;
        ReservationDate = reservationDate;
        ReservationMemberId = reservationMemberId;
        LoanCount = loanCount;
        TotalUsageTime = totalUsageTime;
        IsUnderRepair = isUnderRepair;
    }

    public ToolId ToolId { get; }
    public MemberId OriginalOwnerId { get; }
    public DateTime InitialAcquisitionDate { get; }
    public string SerialNumber { get; }
    public ItemStatus Status { get; private set; }
    public Condition Condition { get; private set; }
    public MemberId? CurrentHolderId { get; private set; }
    public DateTime? LastBorrowedDate { get; private set; }
    public DateTime? ReservationDate { get; private set; }
    public MemberId? ReservationMemberId { get; private set; }
    public int LoanCount { get; private set; }
    public TimeSpan TotalUsageTime { get; private set; }
    public bool IsUnderRepair { get; private set; }

    public static InventoryItem Create(
        ToolId toolId,
        MemberId originalOwnerId,
        DateTime initialAcquisitionDate,
        string serialNumber,
        Condition condition)
    {
        return new(
            new InventoryItemId(default),
            toolId,
            originalOwnerId,
            initialAcquisitionDate,
            serialNumber,
            ItemStatus.Available,
            condition);
    }

    public static InventoryItem Create(
        InventoryItemId id,
        ToolId toolId,
        MemberId originalOwnerId,
        DateTime initialAcquisitionDate,
        string serialNumber,
        ItemStatus status,
        Condition condition,
        MemberId? currentHolderId,
        DateTime? lastBorrowedDate,
        DateTime? reservationDate,
        MemberId? reservationMemberId,
        int loanCount = 0,
        TimeSpan totalUsageTime = default,
        bool isUnderRepair = false)
    {
        return new(
            id,
            toolId,
            originalOwnerId,
            initialAcquisitionDate,
            serialNumber,
            status,
            condition,
            currentHolderId,
            lastBorrowedDate,
            reservationDate,
            reservationMemberId,
            loanCount,
            totalUsageTime,
            isUnderRepair);
    }

    public void Loan(MemberId memberId)
    {
        ArgumentNullException.ThrowIfNull(memberId);

        if (!Status.Equals(ItemStatus.Available))
            throw new InvalidOperationException("Tool is not available for borrowing.");

        if (ReservationDate is not null || ReservationMemberId is not null)
            throw new InvalidOperationException("Tool is reserved and cannot be borrowed.");

        if (IsUnderRepair)
            throw new InvalidOperationException("Tool is under repair and cannot be borrowed.");

        CurrentHolderId = memberId;
        Status = ItemStatus.Loaned;
        LastBorrowedDate = DateTime.UtcNow;
        LoanCount++;

        AddDomainEvent(new ItemLoanedDomainEvent(Id, memberId, LastBorrowedDate.Value));
    }

    public void Return(Condition returnedCondition)
    {
        if (!Status.Equals(ItemStatus.Loaned))
            throw new InvalidOperationException("Tool is not currently loaned out.");

        returnedCondition.ValidateDefined(nameof(returnedCondition)).ValidateNotDefault(nameof(returnedCondition));

        if (LastBorrowedDate.HasValue)
            TotalUsageTime += DateTime.UtcNow - LastBorrowedDate.Value;

        CurrentHolderId = null;
        Status = ItemStatus.Available;
        Condition = returnedCondition;

        AddDomainEvent(new ToolReturnedEvent(Id, CurrentHolderId ?? throw new InvalidOperationException("No current holder to return from."), returnedCondition));
    }

    public void Reserve(DateTime reserveDate, MemberId member)
    {
        if (ReservationDate is not null)
            throw new InvalidOperationException("Tool is already reserved.");

        ArgumentNullException.ThrowIfNull(member);

        if (reserveDate == default || reserveDate <= DateTime.UtcNow)
            throw new ArgumentNullException(nameof(reserveDate));

        if (reserveDate > DateTime.UtcNow.AddDays(28))
            throw new InvalidOperationException("Tool cannot be reserved more than 28 days in advance.");

        ReservationDate = reserveDate;
        ReservationMemberId = member;
    }

    public void ClearReservation()
    {
        ReservationDate = null;
        ReservationMemberId = null;
    }

    public void MarkAsLost(MemberId reporter)
    {
        if (Status.Equals(ItemStatus.Lost))
            throw new InvalidOperationException("Tool is already marked as lost.");

        CurrentHolderId = null;
        Status = ItemStatus.Lost;
    }

    public void MarkForRepair()
    {
        if (IsUnderRepair)
            throw new InvalidOperationException("Tool is already marked for repair.");

        IsUnderRepair = true;
        Status = ItemStatus.UnderMaintenance;
    }

    public void CompleteRepair(Condition newCondition)
    {
        if (!IsUnderRepair)
            throw new InvalidOperationException("Tool is not under repair.");

        newCondition.ValidateDefined(nameof(newCondition)).ValidateNotDefault(nameof(newCondition));

        IsUnderRepair = false;
        Condition = newCondition;
        Status = ItemStatus.Available;
    }
}
