using TSQR.ToolLibrary.Application.Queries;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class ToolQueries(ISqlUnitOfWork uow) : IToolQueries
{
    public async Task<List<Tool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await uow.Connection.QueryAsync<ToolRow>(
            @"SELECT t.Id, t.Model, t.Description, t.ToolType, t.AmortizationRate, t.Metadata,
                     m.Id AS ManufacturerId, m.Name AS ManufacturerName
              FROM Tools t
              INNER JOIN Manufacturers m ON m.Id = t.ManufacturerId
              ORDER BY t.Model"
        );

        return rows.Select(r =>
                Tool.Create(
                    new ToolId(r.Id),
                    r.Model,
                    r.Description,
                    Manufacturer.Create(new ManufacturerId(r.ManufacturerId), r.ManufacturerName),
                    (ToolType)r.ToolType,
                    (AmortizationRate)r.AmortizationRate,
                    r.Metadata
                )
            )
            .ToList();
    }

    public async Task<ToolStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var typeStats = await uow.Connection.QueryAsync<StatsRow>(
            "SELECT ToolType AS Key, COUNT(*)::int AS Count FROM Tools GROUP BY ToolType ORDER BY Key"
        );

        var scarcityStats = await uow.Connection.QueryAsync<StatsRow>(
            "SELECT sl.ScarcityLevel AS Key, COUNT(*)::int AS Count FROM ToolScarcityByLocation sl GROUP BY sl.ScarcityLevel ORDER BY Key"
        );

        return new ToolStats(
            typeStats.Select(s => (s.Key, ToolStats.GetTypeName(s.Key), s.Count)).ToList(),
            scarcityStats.Select(s => (s.Key, ToolStats.GetScarcityName(s.Key), s.Count)).ToList()
        );
    }

    private sealed record ToolRow(
        int Id,
        string Model,
        string Description,
        int ToolType,
        int AmortizationRate,
        string? Metadata,
        int ManufacturerId,
        string ManufacturerName
    );

    private sealed record StatsRow(int Key, int Count);
}
