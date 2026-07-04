using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public sealed class InventoryReadController(
    IInteractor<GetInventoryQuery, PagedResult<InventoryListItem>> list,
    IInteractor<GetInventoryByIdQuery, InventoryListItem?> byId) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<InventoryListItem>> List(
        [FromQuery] int? toolId,
        [FromQuery] int? status,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20)
        => await list.ExecuteAsync(new GetInventoryQuery(toolId, status, page, pageSize));

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryListItem>> Get(int id)
    {
        var result = await byId.ExecuteAsync(new GetInventoryByIdQuery(id));
        return result is null ? NotFound() : result;
    }
}