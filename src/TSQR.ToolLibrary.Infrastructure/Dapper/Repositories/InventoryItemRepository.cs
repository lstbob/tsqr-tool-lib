namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

internal sealed record InventoryItemRow(
    InventoryItemId Id,
    ToolId ToolId,
    MemberId OriginalOwnerId,
    DateTime InitialAcquisitionDate,
    string SerialNumber,
    ItemStatus Status,
    Condition Condition,
    MemberId? CurrentHolderId,
    DateTime? LastBorrowedDate,
    DateTime? ReservationDate,
    MemberId? ReservationMemberId,
    int LoanCount,
    long TotalUsageTimeTicks,
    bool IsUnderRepair);

public class InventoryItemRepository : IRepository<InventoryItem, InventoryItemId>
{
    private readonly DapperUnitOfWork _uow;

    public InventoryItemRepository(DapperUnitOfWork uow) => _uow = uow;

    public IUnitOfWork UnitOfWork => _uow;

    public async Task<InventoryItem?> GetByIdAsync(InventoryItemId id, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var row = await _uow.Connection.QuerySingleOrDefaultAsync<InventoryItemRow>(
            @"SELECT Id, ToolId, OriginalOwnerId, InitialAcquisitionDate, SerialNumber,
                     Status, Condition, CurrentHolderId, LastBorrowedDate,
                     ReservationDate, ReservationMemberId, LoanCount,
                     TotalUsageTimeTicks, IsUnderRepair
              FROM InventoryItems WHERE Id = @Id",
            new { Id = id.Value },
            transaction: _uow.Transaction);

        if (row is null) return null;

        return InventoryItem.Create(
            row.Id,
            row.ToolId,
            row.OriginalOwnerId,
            row.InitialAcquisitionDate,
            row.SerialNumber,
            row.Status,
            row.Condition,
            row.CurrentHolderId,
            row.LastBorrowedDate,
            row.ReservationDate,
            row.ReservationMemberId,
            row.LoanCount,
            TimeSpan.FromTicks(row.TotalUsageTimeTicks),
            row.IsUnderRepair);
    }

    public async Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var id = await _uow.Connection.ExecuteScalarAsync<int>(
            @"INSERT INTO InventoryItems (ToolId, OriginalOwnerId, InitialAcquisitionDate, SerialNumber,
                   Status, Condition, CurrentHolderId, LastBorrowedDate,
                   ReservationDate, ReservationMemberId, LoanCount,
                   TotalUsageTimeTicks, IsUnderRepair)
              VALUES (@ToolId, @OriginalOwnerId, @InitialAcquisitionDate, @SerialNumber,
                   @Status, @Condition, @CurrentHolderId, @LastBorrowedDate,
                   @ReservationDate, @ReservationMemberId, @LoanCount,
                   @TotalUsageTimeTicks, @IsUnderRepair);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                ToolId = entity.ToolId.Value,
                OriginalOwnerId = entity.OriginalOwnerId.Value,
                entity.InitialAcquisitionDate,
                entity.SerialNumber,
                Status = entity.Status,
                Condition = entity.Condition,
                CurrentHolderId = entity.CurrentHolderId?.Value,
                entity.LastBorrowedDate,
                entity.ReservationDate,
                ReservationMemberId = entity.ReservationMemberId?.Value,
                entity.LoanCount,
                TotalUsageTimeTicks = entity.TotalUsageTime.Ticks,
                entity.IsUnderRepair
            },
            transaction: _uow.Transaction);

        entity.SetAssignedId(new InventoryItemId(id));
    }

    public void Update(InventoryItem entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            @"UPDATE InventoryItems
              SET ToolId = @ToolId, OriginalOwnerId = @OriginalOwnerId,
                  InitialAcquisitionDate = @InitialAcquisitionDate,
                  SerialNumber = @SerialNumber, Status = @Status,
                  Condition = @Condition, CurrentHolderId = @CurrentHolderId,
                  LastBorrowedDate = @LastBorrowedDate,
                  ReservationDate = @ReservationDate,
                  ReservationMemberId = @ReservationMemberId,
                  LoanCount = @LoanCount,
                  TotalUsageTimeTicks = @TotalUsageTimeTicks,
                  IsUnderRepair = @IsUnderRepair
              WHERE Id = @Id",
            new
            {
                Id = entity.Id.Value,
                ToolId = entity.ToolId.Value,
                OriginalOwnerId = entity.OriginalOwnerId.Value,
                entity.InitialAcquisitionDate,
                entity.SerialNumber,
                Status = entity.Status,
                Condition = entity.Condition,
                CurrentHolderId = entity.CurrentHolderId?.Value,
                entity.LastBorrowedDate,
                entity.ReservationDate,
                ReservationMemberId = entity.ReservationMemberId?.Value,
                entity.LoanCount,
                TotalUsageTimeTicks = entity.TotalUsageTime.Ticks,
                entity.IsUnderRepair
            },
            transaction: _uow.Transaction);
    }

    public void Delete(InventoryItem entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            "DELETE FROM InventoryItems WHERE Id = @Id",
            new { Id = entity.Id.Value },
            transaction: _uow.Transaction);
    }
}
