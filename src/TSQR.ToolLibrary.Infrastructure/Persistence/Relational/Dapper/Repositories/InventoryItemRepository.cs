using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

internal sealed record MaintenanceRecordRow
{
    public MaintenanceRecordId Id { get; init; }
    public InventoryItemId ItemId { get; init; }
    public MemberId ReportedById { get; init; }
    public DateTime ReportedDate { get; init; }
    public string Description { get; init; }
    public MaintenanceStatus Status { get; init; }
    public MemberId? CompletedById { get; init; }
    public DateTime? CompletedDate { get; init; }
    public Condition? ResultingCondition { get; init; }
}

public sealed class InventoryItemRepository(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<InventoryItem> itemMapping,
    ISqlEntityMapping<MaintenanceRecord> recordMapping,
    ISqlDialect dialect)
    : SqlRepository<InventoryItem, InventoryItemId>(uow, itemMapping, dialect),
        IRepository<InventoryItem, InventoryItemId>
{
    private static readonly MaintenanceStatus[] OpenStatuses =
        [MaintenanceStatus.Reported, MaintenanceStatus.InProgress];

    private static readonly int[] OpenStatusValues =
        [(int)MaintenanceStatus.Reported, (int)MaintenanceStatus.InProgress];

    public override async Task<InventoryItem?> GetByIdAsync(
        InventoryItemId id,
        CancellationToken cancellationToken = default)
    {
        var item = await base.GetByIdAsync(id, cancellationToken);
        if (item is null)
            return null;

        var row = await Database.QuerySingleOrDefaultAsync<MaintenanceRecordRow>(
            @"SELECT Id, ItemId, ReportedById, ReportedDate, Description,
                     Status, CompletedById, CompletedDate, ResultingCondition
              FROM MaintenanceRecords
              WHERE ItemId = @ItemId AND Status = ANY(@Statuses)
              ORDER BY Id DESC LIMIT 1",
            new { ItemId = id.Value, Statuses = OpenStatusValues });

        if (row is not null)
        {
            var recordResult = MaintenanceRecord.Create(
                row.Id,
                row.ItemId,
                row.ReportedById,
                row.ReportedDate,
                row.Description,
                row.Status,
                row.CompletedById,
                row.CompletedDate,
                row.ResultingCondition);

            if (recordResult.IsSuccess)
                item.RehydrateRepair(recordResult.Value);
        }

        return item;
    }

    public override async Task AddAsync(
        InventoryItem entity,
        CancellationToken cancellationToken = default)
    {
        await base.AddAsync(entity, cancellationToken);

        var repair = entity.GetCurrentRepair();
        if (repair is not null)
        {
            var sql = recordMapping.InsertSql + dialect.SelectInsertedIdSuffix;
            var id = await Database.ExecuteScalarAsync<int>(
                sql,
                recordMapping.ToInsertParameters(repair));

            repair.SetAssignedId(new MaintenanceRecordId(id));
        }
    }

    public override void Update(InventoryItem entity)
    {
        base.Update(entity);

        var repair = entity.GetCurrentRepair();
        if (repair is not null)
        {
            if (repair.IsTransient())
            {
                var sql = recordMapping.InsertSql + dialect.SelectInsertedIdSuffix;
                var id = Database.ExecuteScalar<int>(
                    sql,
                    recordMapping.ToInsertParameters(repair));

                repair.SetAssignedId(new MaintenanceRecordId(id));
            }
            else
            {
                Database.Execute(
                    recordMapping.UpdateSql,
                    recordMapping.ToUpdateParameters(repair));
            }
        }
    }
}
