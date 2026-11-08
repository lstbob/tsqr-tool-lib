using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

internal sealed record ToolRow
{
    public ToolId Id { get; init; }
    public string Model { get; init; }
    public string Description { get; init; }
    public ManufacturerId ManufacturerId { get; init; }
    public string ManufacturerName { get; init; }
    public ToolType ToolType { get; init; }
    public AmortizationRate AmortizationRate { get; init; }
    public string? Metadata { get; init; }
    public int CommunityId { get; init; }
    public long RowVersion { get; init; }
}

internal sealed record ScarcityRow
{
    public LocationId LocationId { get; init; }
    public ScarcityLevel ScarcityLevel { get; init; }
}

internal sealed record ToolInsertDto(
    string Model,
    string Description,
    int ManufacturerId,
    ToolType ToolType,
    AmortizationRate AmortizationRate,
    string? Metadata,
    int CommunityId);

internal sealed record ToolUpdateDto(
    int Id,
    string Model,
    string Description,
    int ManufacturerId,
    ToolType ToolType,
    AmortizationRate AmortizationRate,
    string? Metadata,
    int CommunityId,
    long RowVersion);

public sealed class ToolMapping : ISqlEntityMapping<Tool, ToolId>
{
    public string TableName => "Tools";

    public string InsertSql =>
        @"INSERT INTO Tools (Model, Description, ManufacturerId, ToolType, AmortizationRate, Metadata, CommunityId)
          VALUES (@Model, @Description, @ManufacturerId, @ToolType, @AmortizationRate, @Metadata, @CommunityId)
";

    public string UpdateSql =>
        @"UPDATE Tools
          SET Model = @Model, Description = @Description, ManufacturerId = @ManufacturerId,
              ToolType = @ToolType, AmortizationRate = @AmortizationRate, Metadata = @Metadata,
              CommunityId = @CommunityId
          WHERE Id = @Id AND xmin::text::bigint = @RowVersion
          RETURNING xmin::text::bigint";

    public string DeleteSql =>
        "DELETE FROM Tools WHERE Id = @Id AND xmin::text::bigint = @RowVersion";

    public async Task<Tool?> GetByIdAsync(ISqlConnection db, ToolId id)
    {
        var row = await db.QuerySingleOrDefaultAsync<ToolRow>(
            @"SELECT t.Id, t.Model, t.Description, t.ToolType, t.AmortizationRate, t.Metadata, t.CommunityId,
                     m.Id AS ManufacturerId, m.Name AS ManufacturerName,
                     t.xmin::text::bigint AS RowVersion
              FROM Tools t
              INNER JOIN Manufacturers m ON m.Id = t.ManufacturerId
              WHERE t.Id = @Id", new { Id = id.Value });

        if (row is null)
            return null;

        var tool = Tool.Create(
            row.Id,
            row.Model,
            row.Description,
            Manufacturer.Create(row.ManufacturerId, row.ManufacturerName),
            row.ToolType,
            row.AmortizationRate,
            row.Metadata,
            row.CommunityId);

        tool.SetRowVersion(row.RowVersion);

        var scarcityRows = await db.QueryAsync<ScarcityRow>(
            "SELECT LocationId, ScarcityLevel FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = id.Value });

        foreach (var s in scarcityRows)
            tool.SetScarcityLevel(s.LocationId, s.ScarcityLevel);

        return tool;
    }

    public Task<long> GetRowVersionAsync(ISqlConnection db, ToolId id) =>
        db.ExecuteScalarAsync<long>(
            "SELECT xmin::text::bigint FROM Tools WHERE Id = @Id",
            new { Id = id.Value });

    public object ToInsertParameters(Tool entity) => new ToolInsertDto(
        entity.Model,
        entity.Description,
        entity.Manufacturer.Id.Value,
        entity.Type,
        entity.AmortizationRate,
        entity.Metadata,
        entity.CommunityId);

    public object ToUpdateParameters(Tool entity) => new ToolUpdateDto(
        entity.Id.Value,
        entity.Model,
        entity.Description,
        entity.Manufacturer.Id.Value,
        entity.Type,
        entity.AmortizationRate,
        entity.Metadata,
        entity.CommunityId,
        entity.RowVersion);

    public object ToDeleteParameters(Tool entity) =>
        new { Id = entity.Id.Value, RowVersion = entity.RowVersion };

    public ToolId CreateId(object databaseId) => new(Convert.ToInt32(databaseId));
}
