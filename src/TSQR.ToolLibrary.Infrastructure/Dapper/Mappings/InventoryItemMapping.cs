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

internal sealed record InventoryItemInsertDto(
    int ToolId,
    int OriginalOwnerId,
    DateTime InitialAcquisitionDate,
    string SerialNumber,
    ItemStatus Status,
    Condition Condition,
    int? CurrentHolderId,
    DateTime? LastBorrowedDate,
    DateTime? ReservationDate,
    int? ReservationMemberId,
    int LoanCount,
    long TotalUsageTimeTicks,
    bool IsUnderRepair);

internal sealed record InventoryItemUpdateDto(
    int Id,
    int ToolId,
    int OriginalOwnerId,
    DateTime InitialAcquisitionDate,
    string SerialNumber,
    ItemStatus Status,
    Condition Condition,
    int? CurrentHolderId,
    DateTime? LastBorrowedDate,
    DateTime? ReservationDate,
    int? ReservationMemberId,
    int LoanCount,
    long TotalUsageTimeTicks,
    bool IsUnderRepair);

public sealed class InventoryItemMapping : ISqlEntityMapping<InventoryItem>
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
          RETURNING Id";

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

    public async Task<InventoryItem?> GetByIdAsync(ISqlConnection db, object id)
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

    public object ToInsertParameters(InventoryItem entity) => new InventoryItemInsertDto(
        entity.ToolId.Value,
        entity.OriginalOwnerId.Value,
        entity.InitialAcquisitionDate,
        entity.SerialNumber,
        entity.Status,
        entity.Condition,
        entity.CurrentHolderId?.Value,
        entity.LastBorrowedDate,
        entity.ReservationDate,
        entity.ReservationMemberId?.Value,
        entity.LoanCount,
        entity.TotalUsageTime.Ticks,
        entity.IsUnderRepair);

    public object ToUpdateParameters(InventoryItem entity) => new InventoryItemUpdateDto(
        entity.Id.Value,
        entity.ToolId.Value,
        entity.OriginalOwnerId.Value,
        entity.InitialAcquisitionDate,
        entity.SerialNumber,
        entity.Status,
        entity.Condition,
        entity.CurrentHolderId?.Value,
        entity.LastBorrowedDate,
        entity.ReservationDate,
        entity.ReservationMemberId?.Value,
        entity.LoanCount,
        entity.TotalUsageTime.Ticks,
        entity.IsUnderRepair);
}
