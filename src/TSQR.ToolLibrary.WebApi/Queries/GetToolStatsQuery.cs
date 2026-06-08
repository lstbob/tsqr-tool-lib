using MediatR;
using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetToolStatsQuery : IRequest<ToolStatsResult>;

public record ToolStatsResult(List<ToolStatsItem> ByType, List<ToolStatsItem> ByScarcity);

public sealed class GetToolStatsHandler : IRequestHandler<GetToolStatsQuery, ToolStatsResult>
{
    private readonly IToolRepository _toolRepo;

    public GetToolStatsHandler(IToolRepository toolRepo)
    {
        _toolRepo = toolRepo;
    }

    public async Task<ToolStatsResult> Handle(GetToolStatsQuery request, CancellationToken ct)
    {
        var stats = await _toolRepo.GetStatsAsync(ct);

        return new ToolStatsResult(
            stats.ByType.Select(s => new ToolStatsItem(s.Key, s.Label, s.Count)).ToList(),
            stats.ByScarcity.Select(s => new ToolStatsItem(s.Key, s.Label, s.Count)).ToList());
    }
}
