namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

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

public class ToolRepository : IRepository<Tool, ToolId>
{
    private readonly DapperUnitOfWork _uow;

    public ToolRepository(DapperUnitOfWork uow) => _uow = uow;

    public IUnitOfWork UnitOfWork => _uow;

    public async Task<Tool?> GetByIdAsync(ToolId id, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var dto = await _uow.Connection.QuerySingleOrDefaultAsync<ToolRow>(
            @"SELECT t.Id, t.Model, t.Description, t.ToolType, t.AmortizationRate, t.Metadata,
                     m.Id AS ManufacturerId, m.Name AS ManufacturerName
              FROM Tools t
              INNER JOIN Manufacturers m ON m.Id = t.ManufacturerId
              WHERE t.Id = @Id",
            new { Id = id.Value },
            transaction: _uow.Transaction);

        if (dto is null) return null;

        var tool = Tool.Create(
            dto.Id,
            dto.Model,
            dto.Description,
            Manufacturer.Create(dto.ManufacturerId, dto.ManufacturerName),
            dto.ToolType,
            dto.AmortizationRate,
            dto.Metadata);

        var scarcityRows = await _uow.Connection.QueryAsync<ScarcityRow>(
            "SELECT LocationId, ScarcityLevel FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = id.Value },
            transaction: _uow.Transaction);

        foreach (var s in scarcityRows)
            tool.SetScarcityLevel(s.LocationId, s.ScarcityLevel);

        return tool;
    }

    public async Task AddAsync(Tool entity, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var id = await _uow.Connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Tools (Model, Description, ManufacturerId, ToolType, AmortizationRate, Metadata)
              VALUES (@Model, @Description, @ManufacturerId, @ToolType, @AmortizationRate, @Metadata);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                entity.Model,
                entity.Description,
                ManufacturerId = entity.Manufacturer.Id.Value,
                ToolType = entity.Type,
                AmortizationRate = entity.AmortizationRate,
                entity.Metadata
            },
            transaction: _uow.Transaction);

        entity.SetAssignedId(new ToolId(id));

        foreach (var kvp in entity.ScarcityByLocation)
        {
            await _uow.Connection.ExecuteAsync(
                @"INSERT INTO ToolScarcityByLocation (ToolId, LocationId, ScarcityLevel)
                  VALUES (@ToolId, @LocationId, @ScarcityLevel)",
                new { ToolId = id, LocationId = kvp.Key.Value, ScarcityLevel = kvp.Value },
                transaction: _uow.Transaction);
        }
    }

    public void Update(Tool entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            @"UPDATE Tools
              SET Model = @Model, Description = @Description, ManufacturerId = @ManufacturerId,
                  ToolType = @ToolType, AmortizationRate = @AmortizationRate, Metadata = @Metadata
              WHERE Id = @Id",
            new
            {
                Id = entity.Id.Value,
                entity.Model,
                entity.Description,
                ManufacturerId = entity.Manufacturer.Id.Value,
                ToolType = entity.Type,
                AmortizationRate = entity.AmortizationRate,
                entity.Metadata
            },
            transaction: _uow.Transaction);

        _uow.Connection.Execute(
            "DELETE FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = entity.Id.Value },
            transaction: _uow.Transaction);

        foreach (var kvp in entity.ScarcityByLocation)
        {
            _uow.Connection.Execute(
                @"INSERT INTO ToolScarcityByLocation (ToolId, LocationId, ScarcityLevel)
                  VALUES (@ToolId, @LocationId, @ScarcityLevel)",
                new { ToolId = entity.Id.Value, LocationId = kvp.Key.Value, ScarcityLevel = kvp.Value },
                transaction: _uow.Transaction);
        }
    }

    public void Delete(Tool entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            "DELETE FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = entity.Id.Value },
            transaction: _uow.Transaction);

        _uow.Connection.Execute(
            "DELETE FROM Tools WHERE Id = @Id",
            new { Id = entity.Id.Value },
            transaction: _uow.Transaction);
    }
}
