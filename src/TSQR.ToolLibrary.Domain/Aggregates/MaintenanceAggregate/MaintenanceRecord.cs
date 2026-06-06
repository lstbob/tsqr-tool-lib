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
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        ReportedById = reportedById ?? throw new ArgumentNullException(nameof(reportedById));
        ReportedDate = reportedDate;
        Description = description.Validate(nameof(description));
        Status = status.ValidateDefined(nameof(status)).ValidateNotDefault(nameof(status));
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

    public static MaintenanceRecord Create(
        InventoryItemId itemId,
        MemberId reportedById,
        string description)
    {
        return new(
            new MaintenanceRecordId(default),
            itemId,
            reportedById,
            DateTime.UtcNow,
            description,
            MaintenanceStatus.Reported);
    }

    public static MaintenanceRecord Create(
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
        return new(
            id,
            itemId,
            reportedById,
            reportedDate,
            description,
            status,
            completedById,
            completedDate,
            resultingCondition);
    }

    public void StartWork()
    {
        if (Status != MaintenanceStatus.Reported)
            throw new InvalidOperationException("Only reported maintenance can be started.");

        Status = MaintenanceStatus.InProgress;
    }

    public void Complete(MemberId completedById, Condition resultingCondition)
    {
        if (Status != MaintenanceStatus.InProgress)
            throw new InvalidOperationException("Only in-progress maintenance can be completed.");

        ArgumentNullException.ThrowIfNull(completedById);
        resultingCondition.ValidateDefined(nameof(resultingCondition)).ValidateNotDefault(nameof(resultingCondition));

        Status = MaintenanceStatus.Completed;
        CompletedById = completedById;
        CompletedDate = DateTime.UtcNow;
        ResultingCondition = resultingCondition;

        AddDomainEvent(new ToolRepairedEvent(ItemId, completedById, resultingCondition));
    }
}
