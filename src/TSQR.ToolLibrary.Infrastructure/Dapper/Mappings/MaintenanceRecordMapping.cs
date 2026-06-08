namespace TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

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

public sealed class MaintenanceRecordMapping : IEntityMapping<MaintenanceRecord>
{
    public string TableName => "MaintenanceRecords";

    public string InsertSql =>
        @"INSERT INTO MaintenanceRecords (ItemId, ReportedById, ReportedDate, Description, Status,
                CompletedById, CompletedDate, ResultingCondition)
          VALUES (@ItemId, @ReportedById, @ReportedDate, @Description, @Status,
                @CompletedById, @CompletedDate, @ResultingCondition);
          SELECT CAST(SCOPE_IDENTITY() AS INT)";

    public string UpdateSql =>
        @"UPDATE MaintenanceRecords
          SET ItemId = @ItemId, ReportedById = @ReportedById,
              ReportedDate = @ReportedDate, Description = @Description,
              Status = @Status, CompletedById = @CompletedById,
              CompletedDate = @CompletedDate, ResultingCondition = @ResultingCondition
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM MaintenanceRecords WHERE Id = @Id";

    public async Task<MaintenanceRecord?> GetByIdAsync(IDatabaseConnection db, object id)
    {
        var row = await db.QuerySingleOrDefaultAsync<MaintenanceRecordRow>(
            @"SELECT Id, ItemId, ReportedById, ReportedDate, Description,
                     Status, CompletedById, CompletedDate, ResultingCondition
              FROM MaintenanceRecords WHERE Id = @Id", new { Id = id });

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

    public object ToInsertParameters(MaintenanceRecord entity) => new
    {
        ItemId = entity.ItemId.Value,
        ReportedById = entity.ReportedById.Value,
        entity.ReportedDate,
        entity.Description,
        Status = entity.Status,
        CompletedById = entity.CompletedById?.Value,
        entity.CompletedDate,
        ResultingCondition = entity.ResultingCondition
    };

    public object ToUpdateParameters(MaintenanceRecord entity) => new
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
    };
}
