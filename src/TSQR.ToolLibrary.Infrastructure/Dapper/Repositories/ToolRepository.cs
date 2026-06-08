using TSQR.ToolLibrary.Domain;

namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

public sealed class ToolRepository : Repository<Tool, ToolId>, IToolRepository
{
    public ToolRepository(IDatabaseUnitOfWork uow, IEntityMapping<Tool> mapping) : base(uow, mapping)
    {
    }

    public async Task<List<Tool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await Database.QueryAsync<ToolRow>(
            @"SELECT t.Id, t.Model, t.Description, t.ToolType, t.AmortizationRate, t.Metadata,
                     m.Id AS ManufacturerId, m.Name AS ManufacturerName
              FROM Tools t
              INNER JOIN Manufacturers m ON m.Id = t.ManufacturerId
              ORDER BY t.Model");

        return rows.Select(r => Tool.Create(
            new ToolId(r.Id),
            r.Model,
            r.Description,
            Manufacturer.Create(new ManufcaturerId(r.ManufacturerId), r.ManufacturerName),
            (ToolType)r.ToolType,
            (AmortizationRate)r.AmortizationRate,
            r.Metadata)).ToList();
    }

    public async Task<ToolStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var typeStats = await Database.QueryAsync<StatsRow>(
            "SELECT ToolType AS Key, COUNT(*) AS Count FROM Tools GROUP BY ToolType ORDER BY Key");

        var scarcityStats = await Database.QueryAsync<StatsRow>(
            "SELECT sl.ScarcityLevel AS Key, COUNT(*) AS Count FROM ToolScarcityByLocation sl GROUP BY sl.ScarcityLevel ORDER BY Key");

        return new ToolStats(
            typeStats.Select(s => (s.Key, ToolStats.GetTypeName(s.Key), s.Count)).ToList(),
            scarcityStats.Select(s => (s.Key, ToolStats.GetScarcityName(s.Key), s.Count)).ToList());
    }

    public override async Task AddAsync(Tool entity, CancellationToken cancellationToken = default)
    {
        await base.AddAsync(entity, cancellationToken);

        foreach (var kvp in entity.ScarcityByLocation)
            await Database.ExecuteAsync(
                @"INSERT INTO ToolScarcityByLocation (ToolId, LocationId, ScarcityLevel)
                  VALUES (@ToolId, @LocationId, @ScarcityLevel)",
                new { ToolId = entity.Id.Value, LocationId = kvp.Key.Value, ScarcityLevel = kvp.Value });
    }

    public override void Update(Tool entity)
    {
        base.Update(entity);

        Database.Execute(
            "DELETE FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = entity.Id.Value });

        foreach (var kvp in entity.ScarcityByLocation)
            Database.Execute(
                @"INSERT INTO ToolScarcityByLocation (ToolId, LocationId, ScarcityLevel)
                  VALUES (@ToolId, @LocationId, @ScarcityLevel)",
                new { ToolId = entity.Id.Value, LocationId = kvp.Key.Value, ScarcityLevel = kvp.Value });
    }

    public override void Delete(Tool entity)
    {
        Database.Execute(
            "DELETE FROM ToolScarcityByLocation WHERE ToolId = @Id",
            new { Id = entity.Id.Value });

        base.Delete(entity);
    }

    private sealed record ToolRow(
        int Id, string Model, string Description, int ToolType,
        int AmortizationRate, string? Metadata,
        int ManufacturerId, string ManufacturerName);

    private sealed record StatsRow(int Key, int Count);
}
