namespace TSQR.ToolLibrary.Domain.Aggregates.MaintenanceAggregate;

public class MaintenanceRecord : Entity<MaintenanceRecordId>, IAggregateRoot
{
    private MaintenanceRecord(
        MaintenanceRecordId id,
        InventoryItemId itemId,
        MemberId reportedById,
        DateTime reportedDate,
        string description,
        MaintenanceStatus status,
        MemberId? completedById = null,
        DateTime? completedDate = null,
        Condition? resultingCondition = null) : base(id)
    {
        ItemId = itemId;
        ReportedById = reportedById;
        ReportedDate = reportedDate;
        Description = description;
        Status = status;
        CompletedById = completedById;
        CompletedDate = completedDate;
        ResultingCondition = resultingCondition;
    }

    public InventoryItemId ItemId { get; }
    public MemberId ReportedById { get; }
    public DateTime ReportedDate { get; }
    public string Description { get; }
    public MaintenanceStatus Status { get; private set; }
    public MemberId? CompletedById { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public Condition? ResultingCondition { get; private set; }

    public static Result<MaintenanceRecord> Create(
        InventoryItemId itemId,
        MemberId reportedById,
        string description)
    {
        if (itemId is null)
            return new ValidationError(nameof(itemId), "Item ID is required.");
        if (reportedById is null)
            return new ValidationError(nameof(reportedById), "Reported by ID is required.");

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        return new MaintenanceRecord(
            new MaintenanceRecordId(default),
            itemId,
            reportedById,
            DateTime.UtcNow,
            descriptionResult.Value,
            MaintenanceStatus.Reported);
    }

    public static Result<MaintenanceRecord> Create(
        MaintenanceRecordId id,
        InventoryItemId itemId,
        MemberId reportedById,
        DateTime reportedDate,
        string description,
        MaintenanceStatus status,
        MemberId? completedById,
        DateTime? completedDate,
        Condition? resultingCondition)
    {
        if (itemId is null)
            return new ValidationError(nameof(itemId), "Item ID is required.");
        if (reportedById is null)
            return new ValidationError(nameof(reportedById), "Reported by ID is required.");

        var descriptionResult = description.Validate(nameof(description));
        if (descriptionResult.IsFailure)
            return descriptionResult.Error;

        return new MaintenanceRecord(
            id,
            itemId,
            reportedById,
            reportedDate,
            descriptionResult.Value,
            status,
            completedById,
            completedDate,
            resultingCondition);
    }

    public Result StartWork()
    {
        if (Status != MaintenanceStatus.Reported)
            return new DomainError(nameof(Status), "Only reported maintenance can be started.");

        Status = MaintenanceStatus.InProgress;
        return Result.Success();
    }

    public Result Complete(MemberId completedById, Condition resultingCondition)
    {
        if (Status != MaintenanceStatus.InProgress)
            return new DomainError(nameof(Status), "Only in-progress maintenance can be completed.");
        if (completedById is null)
            return new ValidationError(nameof(completedById), "Completed by ID is required.");

        var conditionResult = resultingCondition.ValidateDefined(nameof(resultingCondition));
        if (conditionResult.IsFailure)
            return conditionResult.Error;

        var notDefaultResult = resultingCondition.ValidateNotDefault(nameof(resultingCondition));
        if (notDefaultResult.IsFailure)
            return notDefaultResult.Error;

        Status = MaintenanceStatus.Completed;
        CompletedById = completedById;
        CompletedDate = DateTime.UtcNow;
        ResultingCondition = resultingCondition;

        AddDomainEvent(new ToolRepairedEvent(ItemId, completedById, resultingCondition));
        return Result.Success();
    }
}
