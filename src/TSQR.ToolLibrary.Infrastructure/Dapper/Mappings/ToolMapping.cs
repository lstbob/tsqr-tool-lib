namespace TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

internal sealed record ToolRow(
    ToolId Id,
    string Model,
    string Description,
    ManufcaturerId ManufacturerId,
    string ManufacturerName,
    ToolType ToolType,
    AmortizationRate AmortizationRate,
    string? Metadata);

internal sealed record ScarcityRow(LocationId LocationId, ScarcityLevel ScarcityLevel);

public sealed class ToolMapping : IEntityMapping<Tool>
{
    public string TableName => "Tools";

    public string InsertSql =>
        @"INSERT INTO Tools (Model, Description, ManufacturerId, ToolType, AmortizationRate, Metadata)
          VALUES (@Model, @Description, @ManufacturerId, @ToolType, @AmortizationRate, @Metadata);
          SELECT CAST(SCOPE_IDENTITY() AS INT)";

    public string UpdateSql =>
        @"UPDATE Tools
          SET Model = @Model, Description = @Description, ManufacturerId = @ManufacturerId,
              ToolType = @ToolType, AmortizationRate = @AmortizationRate, Metadata = @Metadata
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM Tools WHERE Id = @Id";

    public async Task<Tool?> GetByIdAsync(IDatabaseConnection db, object id)
    {
        var row = await db.QuerySingleOrDefaultAsync<ToolRow>(
            @"SELECT t.Id, t.Model, t.Description, t.ToolType, t.AmortizationRate, t.Metadata,
                     m.Id AS ManufacturerId, m.Name AS ManufacturerName
              FROM Tools t
              INNER JOIN Manufacturers m ON m.Id = t.ManufacturerId
              WHERE t.Id = @Id", new { Id = id });

        if (row is null) return null;

        var tool = Tool.Create(
            row.Id,
            row.Model,
            row.Description,
            Manufacturer.Create(row.ManufacturerId, row.ManufacturerName),
            row.ToolType,
            row.AmortizationRate,
            row.Metadata);

        var scarcityRows = await db.QueryAsync<ScarcityRow>(
            "SELECT LocationId, ScarcityLevel FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = id });

        foreach (var s in scarcityRows)
            tool.SetScarcityLevel(s.LocationId, s.ScarcityLevel);

        return tool;
    }

    public object ToInsertParameters(Tool entity) => new
    {
        entity.Model,
        entity.Description,
        ManufacturerId = entity.Manufacturer.Id.Value,
        ToolType = entity.Type,
        AmortizationRate = entity.AmortizationRate,
        entity.Metadata
    };

    public object ToUpdateParameters(Tool entity) => new
    {
        Id = entity.Id.Value,
        entity.Model,
        entity.Description,
        ManufacturerId = entity.Manufacturer.Id.Value,
        ToolType = entity.Type,
        AmortizationRate = entity.AmortizationRate,
        entity.Metadata
    };
}
