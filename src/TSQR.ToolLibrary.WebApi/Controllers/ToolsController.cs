using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[AllowAnonymous] // public read-only tool catalog (consumed unauthenticated by the UI)
[Route("api/tools")]
public sealed class ToolsController : ControllerBase
{
    private readonly IInteractor<GetToolsQuery, PagedResult<ToolListItem>> _getTools;
    private readonly IInteractor<GetToolByIdQuery, ToolDetail?> _getToolById;
    private readonly IInteractor<GetToolStatsQuery, ToolStatsResult> _getToolStats;

    public ToolsController(
        IInteractor<GetToolsQuery, PagedResult<ToolListItem>> getTools,
        IInteractor<GetToolByIdQuery, ToolDetail?> getToolById,
        IInteractor<GetToolStatsQuery, ToolStatsResult> getToolStats)
    {
        _getTools = getTools;
        _getToolById = getToolById;
        _getToolStats = getToolStats;
    }

    [HttpGet]
    public async Task<PagedResult<ToolListItem>> GetTools(
        [FromQuery] string? q,
        [FromQuery] int? type,
        [FromQuery] int? manufacturerId,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20)
    {
        return await _getTools.ExecuteAsync(new GetToolsQuery(q, type, manufacturerId, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ToolDetail>> GetTool(int id)
    {
        var result = await _getToolById.ExecuteAsync(new GetToolByIdQuery(id));
        if (result is null) return NotFound();
        return result;
    }

    [HttpGet("stats")]
    public async Task<ToolStatsResult> GetStats()
    {
        return await _getToolStats.ExecuteAsync(new GetToolStatsQuery());
    }
}
