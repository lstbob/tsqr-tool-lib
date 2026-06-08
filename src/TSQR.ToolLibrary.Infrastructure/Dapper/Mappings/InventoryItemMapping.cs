namespace TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

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

public sealed class InventoryItemMapping : IEntityMapping<InventoryItem>
{
    public string TableName => "InventoryItems";

    public string InsertSql =>
        @"INSERT INTO InventoryItems (ToolId, OriginalOwnerId, InitialAcquisitionDate, SerialNumber,
                Status, Condition, CurrentHolderId, LastBorrowedDate,
                ReservationDate, ReservationMemberId, LoanCount,
                TotalUsageTimeTicks, IsUnderRepair)
          VALUES (@ToolId, @OriginalOwnerId, @InitialAcquisitionDate, @SerialNumber,
                @Status, @Condition, @CurrentHolderId, @LastBorrowedDate,
                @ReservationDate, @ReservationMemberId, @LoanCount,
                @TotalUsageTimeTicks, @IsUnderRepair);
          SELECT CAST(SCOPE_IDENTITY() AS INT)";

    public string UpdateSql =>
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
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM InventoryItems WHERE Id = @Id";

    public async Task<InventoryItem?> GetByIdAsync(IDatabaseConnection db, object id)
    {
        var row = await db.QuerySingleOrDefaultAsync<InventoryItemRow>(
            @"SELECT Id, ToolId, OriginalOwnerId, InitialAcquisitionDate, SerialNumber,
                     Status, Condition, CurrentHolderId, LastBorrowedDate,
                     ReservationDate, ReservationMemberId, LoanCount,
                     TotalUsageTimeTicks, IsUnderRepair
              FROM InventoryItems WHERE Id = @Id", new { Id = id });

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

    public object ToInsertParameters(InventoryItem entity) => new
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
    };

    public object ToUpdateParameters(InventoryItem entity) => new
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
    };
}
