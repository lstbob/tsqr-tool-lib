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
        int loanCount = 0,
        TimeSpan totalUsageTime = default,
        bool isUnderRepair = false,
        int communityId = 0) : base(id)
    {
        ToolId = toolId;
        OriginalOwnerId = originalOwnerId;
        InitialAcquisitionDate = initialAcquisitionDate;
        SerialNumber = serialNumber;
        Status = status;
        Condition = condition;
        CurrentHolderId = currentHolderId;
        LastBorrowedDate = lastBorrowedDate;
        LoanCount = loanCount;
        TotalUsageTime = totalUsageTime;
        IsUnderRepair = isUnderRepair;
        CommunityId = communityId;
    }

    public ToolId ToolId { get; }
    public MemberId OriginalOwnerId { get; }
    public DateTime InitialAcquisitionDate { get; }
    public string SerialNumber { get; }
    public ItemStatus Status { get; private set; }
    public Condition Condition { get; private set; }
    public MemberId? CurrentHolderId { get; private set; }
    public DateTime? LastBorrowedDate { get; private set; }
    public int LoanCount { get; private set; }
    public TimeSpan TotalUsageTime { get; private set; }
    public bool IsUnderRepair { get; private set; }
    public int CommunityId { get; private set; }

    public static Result<InventoryItem> Create(
        ToolId toolId,
        MemberId originalOwnerId,
        DateTime initialAcquisitionDate,
        string serialNumber,
        Condition condition,
        int communityId = 0)
    {
        if (toolId is null)
            return new ValidationError(nameof(toolId), "Tool ID is required.");
        if (originalOwnerId is null)
            return new ValidationError(nameof(originalOwnerId), "Original owner ID is required.");

        var acquisitionResult = initialAcquisitionDate.Validate(nameof(initialAcquisitionDate));
        if (acquisitionResult.IsFailure) return acquisitionResult.Error;

        var notFutureResult = initialAcquisitionDate.ValidateNotInFuture(nameof(initialAcquisitionDate));
        if (notFutureResult.IsFailure) return notFutureResult.Error;

        var serialResult = serialNumber.Validate(nameof(serialNumber));
        if (serialResult.IsFailure) return serialResult.Error;

        var statusResult = ItemStatus.Available.ValidateDefined(nameof(ItemStatus));
        if (statusResult.IsFailure) return statusResult.Error;

        var conditionResult = condition.ValidateDefined(nameof(condition));
        if (conditionResult.IsFailure) return conditionResult.Error;

        var conditionNotDefaultResult = condition.ValidateNotDefault(nameof(condition));
        if (conditionNotDefaultResult.IsFailure) return conditionNotDefaultResult.Error;

        return new InventoryItem(
            new InventoryItemId(default),
            toolId,
            originalOwnerId,
            initialAcquisitionDate,
            serialResult.Value,
            ItemStatus.Available,
            condition,
            communityId: communityId);
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
        int loanCount = 0,
        TimeSpan totalUsageTime = default,
        bool isUnderRepair = false,
        int communityId = 0)
    {
        return new InventoryItem(
            id,
            toolId,
            originalOwnerId,
            initialAcquisitionDate,
            serialNumber,
            status,
            condition,
            currentHolderId,
            lastBorrowedDate,
            loanCount,
            totalUsageTime,
            isUnderRepair,
            communityId);
    }

    public Result Loan(MemberId memberId)
    {
        if (memberId is null)
            return new ValidationError(nameof(memberId), "Member ID is required.");

        if (IsUnderRepair)
            return new DomainError(nameof(IsUnderRepair), "Tool is under repair and cannot be borrowed.");

        if (Status == ItemStatus.Loaned)
            return new DomainError(nameof(Status), "Tool is already loaned out.");

        if (Status == ItemStatus.Lost)
            return new DomainError(nameof(Status), "Tool is lost and cannot be borrowed.");

        CurrentHolderId = memberId;
        Status = ItemStatus.Loaned;
        LastBorrowedDate = DateTime.UtcNow;
        LoanCount++;

        AddDomainEvent(new ItemLoanedDomainEvent(Id, memberId, LastBorrowedDate.Value));
        return Result.Success();
    }

    public Result Return(Condition returnedCondition)
    {
        if (!Status.Equals(ItemStatus.Loaned))
            return new DomainError(nameof(Status), "Tool is not currently loaned out.");

        var conditionResult = returnedCondition.ValidateDefined(nameof(returnedCondition));
        if (conditionResult.IsFailure) return conditionResult.Error;

        var notDefaultResult = returnedCondition.ValidateNotDefault(nameof(returnedCondition));
        if (notDefaultResult.IsFailure) return notDefaultResult.Error;

        if (LastBorrowedDate.HasValue)
            TotalUsageTime += DateTime.UtcNow - LastBorrowedDate.Value;

        var previousHolderId = CurrentHolderId;
        CurrentHolderId = null;
        Status = ItemStatus.Available;
        Condition = returnedCondition;

        AddDomainEvent(new ToolReturnedEvent(Id, previousHolderId!, returnedCondition));
        return Result.Success();
    }

    public Result Reserve()
    {
        if (Status != ItemStatus.Available)
            return new DomainError(nameof(Status), "Only available tools can be reserved.");

        Status = ItemStatus.Reserved;
        return Result.Success();
    }

    public Result MarkAsLost(MemberId reporter)
    {
        if (Status.Equals(ItemStatus.Lost))
            return new DomainError(nameof(Status), "Tool is already marked as lost.");

        CurrentHolderId = null;
        Status = ItemStatus.Lost;
        return Result.Success();
    }

    public Result MarkForRepair(MemberId reportedById, string description)
    {
        if (IsUnderRepair)
            return new DomainError(nameof(IsUnderRepair), "Tool is already marked for repair.");

        if (reportedById is null)
            return new ValidationError(nameof(reportedById), "Reported by ID is required.");

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        IsUnderRepair = true;
        Status = ItemStatus.UnderMaintenance;

        AddDomainEvent(new ToolMarkedForRepairEvent(Id, reportedById, descriptionResult.Value));
        return Result.Success();
    }

    public Result CompleteRepair(Condition newCondition)
    {
        if (!IsUnderRepair)
            return new DomainError(nameof(IsUnderRepair), "Tool is not under repair.");

        var conditionResult = newCondition.ValidateDefined(nameof(newCondition));
        if (conditionResult.IsFailure) return conditionResult.Error;

        var notDefaultResult = newCondition.ValidateNotDefault(nameof(newCondition));
        if (notDefaultResult.IsFailure) return notDefaultResult.Error;

        IsUnderRepair = false;
        Condition = newCondition;
        Status = ItemStatus.Available;
        return Result.Success();
    }
}
