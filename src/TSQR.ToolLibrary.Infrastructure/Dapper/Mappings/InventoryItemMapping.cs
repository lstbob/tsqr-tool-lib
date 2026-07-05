namespace TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

internal sealed record InventoryItemRow
{
    public InventoryItemId Id { get; init; }
    public ToolId ToolId { get; init; }
    public MemberId OriginalOwnerId { get; init; }
    public DateTime InitialAcquisitionDate { get; init; }
    public string SerialNumber { get; init; }
    public ItemStatus Status { get; init; }
    public Condition Condition { get; init; }
    public MemberId? CurrentHolderId { get; init; }
    public DateTime? LastBorrowedDate { get; init; }
    public DateTime? ReservationDate { get; init; }
    public MemberId? ReservationMemberId { get; init; }
    public int LoanCount { get; init; }
    public long TotalUsageTimeTicks { get; init; }
    public bool IsUnderRepair { get; init; }
    public int CommunityId { get; init; }
}

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
    bool IsUnderRepair,
    int CommunityId);

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
    bool IsUnderRepair,
    int CommunityId);

public sealed class InventoryItemMapping : ISqlEntityMapping<InventoryItem>
{
    public string TableName => "InventoryItems";

    public string InsertSql =>
        @"INSERT INTO InventoryItems (ToolId, OriginalOwnerId, InitialAcquisitionDate, SerialNumber,
                Status, Condition, CurrentHolderId, LastBorrowedDate,
                ReservationDate, ReservationMemberId, LoanCount,
                TotalUsageTimeTicks, IsUnderRepair, CommunityId)
          VALUES (@ToolId, @OriginalOwnerId, @InitialAcquisitionDate, @SerialNumber,
                @Status, @Condition, @CurrentHolderId, @LastBorrowedDate,
                @ReservationDate, @ReservationMemberId, @LoanCount,
                @TotalUsageTimeTicks, @IsUnderRepair, @CommunityId)
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
              IsUnderRepair = @IsUnderRepair,
              CommunityId = @CommunityId
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM InventoryItems WHERE Id = @Id";

    public async Task<InventoryItem?> GetByIdAsync(ISqlConnection db, int id)
    {
        var row = await db.QuerySingleOrDefaultAsync<InventoryItemRow>(
            @"SELECT Id, ToolId, OriginalOwnerId, InitialAcquisitionDate, SerialNumber,
                     Status, Condition, CurrentHolderId, LastBorrowedDate,
                     ReservationDate, ReservationMemberId, LoanCount,
                     TotalUsageTimeTicks, IsUnderRepair, CommunityId
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
            row.IsUnderRepair,
            row.CommunityId);
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
        entity.IsUnderRepair,
        entity.CommunityId);

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
        entity.IsUnderRepair,
        entity.CommunityId);
}
