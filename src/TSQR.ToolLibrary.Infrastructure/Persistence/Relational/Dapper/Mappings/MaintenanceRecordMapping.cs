using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

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
    public long RowVersion { get; init; }
}

internal sealed record MaintenanceRecordInsertDto(
    int ItemId,
    int ReportedById,
    DateTime ReportedDate,
    string Description,
    MaintenanceStatus Status,
    int? CompletedById,
    DateTime? CompletedDate,
    Condition? ResultingCondition);

internal sealed record MaintenanceRecordUpdateDto(
    int Id,
    int ItemId,
    int ReportedById,
    DateTime ReportedDate,
    string Description,
    MaintenanceStatus Status,
    int? CompletedById,
    DateTime? CompletedDate,
    Condition? ResultingCondition,
    long RowVersion);

public sealed class MaintenanceRecordMapping : ISqlEntityMapping<MaintenanceRecord, MaintenanceRecordId>
{
    public string TableName => "MaintenanceRecords";

    public string InsertSql =>
        @"INSERT INTO MaintenanceRecords (ItemId, ReportedById, ReportedDate, Description, Status,
                CompletedById, CompletedDate, ResultingCondition)
          VALUES (@ItemId, @ReportedById, @ReportedDate, @Description, @Status,
                @CompletedById, @CompletedDate, @ResultingCondition)
";

    public string UpdateSql =>
        @"UPDATE MaintenanceRecords
          SET ItemId = @ItemId, ReportedById = @ReportedById,
              ReportedDate = @ReportedDate, Description = @Description,
              Status = @Status, CompletedById = @CompletedById,
              CompletedDate = @CompletedDate, ResultingCondition = @ResultingCondition
          WHERE Id = @Id AND xmin::text::bigint = @RowVersion
          RETURNING xmin::text::bigint";

    public string DeleteSql =>
        "DELETE FROM MaintenanceRecords WHERE Id = @Id AND xmin::text::bigint = @RowVersion";

    public async Task<MaintenanceRecord?> GetByIdAsync(ISqlConnection db, MaintenanceRecordId id)
    {
        var row = await db.QuerySingleOrDefaultAsync<MaintenanceRecordRow>(
            @"SELECT Id, ItemId, ReportedById, ReportedDate, Description,
                     Status, CompletedById, CompletedDate, ResultingCondition,
                     xmin::text::bigint AS RowVersion
              FROM MaintenanceRecords WHERE Id = @Id", new { Id = id.Value });

        if (row is null) return null;

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

        if (!recordResult.IsSuccess)
            return null;

        recordResult.Value.SetRowVersion(row.RowVersion);
        return recordResult.Value;
    }

    public Task<long> GetRowVersionAsync(ISqlConnection db, MaintenanceRecordId id) =>
        db.ExecuteScalarAsync<long>(
            "SELECT xmin::text::bigint FROM MaintenanceRecords WHERE Id = @Id",
            new { Id = id.Value });

    public object ToInsertParameters(MaintenanceRecord entity) => new MaintenanceRecordInsertDto(
        entity.ItemId.Value,
        entity.ReportedById.Value,
        entity.ReportedDate,
        entity.Description,
        entity.Status,
        entity.CompletedById?.Value,
        entity.CompletedDate,
        entity.ResultingCondition);

    public object ToUpdateParameters(MaintenanceRecord entity) => new MaintenanceRecordUpdateDto(
        entity.Id.Value,
        entity.ItemId.Value,
        entity.ReportedById.Value,
        entity.ReportedDate,
        entity.Description,
        entity.Status,
        entity.CompletedById?.Value,
        entity.CompletedDate,
        entity.ResultingCondition,
        entity.RowVersion);

    public object ToDeleteParameters(MaintenanceRecord entity) =>
        new { Id = entity.Id.Value, RowVersion = entity.RowVersion };

    public MaintenanceRecordId CreateId(object databaseId) => new(Convert.ToInt32(databaseId));
}
