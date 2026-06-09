using MediatR;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Route("api/tools")]
public sealed class ToolsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ToolsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<PagedResult<ToolListItem>> GetTools(
        [FromQuery] string? q,
        [FromQuery] int? type,
        [FromQuery] int? manufacturerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _mediator.Send(new GetToolsQuery(q, type, manufacturerId, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ToolDetail>> GetTool(int id)
    {
        var result = await _mediator.Send(new GetToolByIdQuery(id));
        if (result is null) return NotFound();
        return result;
    }

    [HttpGet("stats")]
    public async Task<ToolStatsResult> GetStats()
    {
        return await _mediator.Send(new GetToolStatsQuery());
    }
}
