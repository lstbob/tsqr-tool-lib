using ToolAggregateRoot = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;

namespace TSQR.ToolLibrary.Application.Queries;

/// <summary>
/// Read-side queries for tool screens and statistics. These return domain
/// aggregates or read models but are intentionally separate from
/// <see cref="TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.IToolRepository"/>.
/// </summary>
public interface IToolQueries
{
    Task<List<ToolAggregateRoot>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ToolStats> GetStatsAsync(CancellationToken cancellationToken = default);
}
