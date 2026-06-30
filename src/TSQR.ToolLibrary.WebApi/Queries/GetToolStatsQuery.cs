using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetToolStatsQuery;

public record ToolStatsResult(List<ToolStatsItem> ByType, List<ToolStatsItem> ByScarcity);

public sealed class GetToolStatsHandler(IToolRepository toolRepo)
    : IInteractor<GetToolStatsQuery, ToolStatsResult>
{
    public async Task<ToolStatsResult> ExecuteAsync(GetToolStatsQuery request, CancellationToken ct)
    {
        var stats = await toolRepo.GetStatsAsync(ct);

        return new ToolStatsResult(
            stats.ByType.Select(s => new ToolStatsItem(s.Key, s.Label, s.Count)).ToList(),
            stats.ByScarcity.Select(s => new ToolStatsItem(s.Key, s.Label, s.Count)).ToList()
        );
    }
}
