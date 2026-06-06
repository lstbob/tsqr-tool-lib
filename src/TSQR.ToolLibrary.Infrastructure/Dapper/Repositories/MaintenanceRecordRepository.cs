namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

internal sealed record MaintenanceRecordRow(
    MaintenanceRecordId Id,
    InventoryItemId ItemId,
    MemberId ReportedById,
    DateTime ReportedDate,
    string Description,
    MaintenanceStatus Status,
    MemberId? CompletedById,
    DateTime? CompletedDate,
    Condition? ResultingCondition);

public class MaintenanceRecordRepository : IRepository<MaintenanceRecord, MaintenanceRecordId>
{
    private readonly DapperUnitOfWork _uow;

    public MaintenanceRecordRepository(DapperUnitOfWork uow) => _uow = uow;

    public IUnitOfWork UnitOfWork => _uow;

    public async Task<MaintenanceRecord?> GetByIdAsync(MaintenanceRecordId id, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var row = await _uow.Connection.QuerySingleOrDefaultAsync<MaintenanceRecordRow>(
            @"SELECT Id, ItemId, ReportedById, ReportedDate, Description,
                     Status, CompletedById, CompletedDate, ResultingCondition
              FROM MaintenanceRecords WHERE Id = @Id",
            new { Id = id.Value },
            transaction: _uow.Transaction);

        if (row is null) return null;

        return MaintenanceRecord.Create(
            row.Id,
            row.ItemId,
            row.ReportedById,
            row.ReportedDate,
            row.Description,
            row.Status,
            row.CompletedById,
            row.CompletedDate,
            row.ResultingCondition);
    }

    public async Task AddAsync(MaintenanceRecord entity, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var id = await _uow.Connection.ExecuteScalarAsync<int>(
            @"INSERT INTO MaintenanceRecords (ItemId, ReportedById, ReportedDate, Description, Status,
                   CompletedById, CompletedDate, ResultingCondition)
              VALUES (@ItemId, @ReportedById, @ReportedDate, @Description, @Status,
                   @CompletedById, @CompletedDate, @ResultingCondition);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                ItemId = entity.ItemId.Value,
                ReportedById = entity.ReportedById.Value,
                entity.ReportedDate,
                entity.Description,
                Status = entity.Status,
                CompletedById = entity.CompletedById?.Value,
                entity.CompletedDate,
                ResultingCondition = entity.ResultingCondition
            },
            transaction: _uow.Transaction);

        entity.SetAssignedId(new MaintenanceRecordId(id));
    }

    public void Update(MaintenanceRecord entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            @"UPDATE MaintenanceRecords
              SET ItemId = @ItemId, ReportedById = @ReportedById,
                  ReportedDate = @ReportedDate, Description = @Description,
                  Status = @Status, CompletedById = @CompletedById,
                  CompletedDate = @CompletedDate, ResultingCondition = @ResultingCondition
              WHERE Id = @Id",
            new
            {
                Id = entity.Id.Value,
                ItemId = entity.ItemId.Value,
                ReportedById = entity.ReportedById.Value,
                entity.ReportedDate,
                entity.Description,
                Status = entity.Status,
                CompletedById = entity.CompletedById?.Value,
                entity.CompletedDate,
                ResultingCondition = entity.ResultingCondition
            },
            transaction: _uow.Transaction);
    }

    public void Delete(MaintenanceRecord entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            "DELETE FROM MaintenanceRecords WHERE Id = @Id",
            new { Id = entity.Id.Value },
            transaction: _uow.Transaction);
    }
}
