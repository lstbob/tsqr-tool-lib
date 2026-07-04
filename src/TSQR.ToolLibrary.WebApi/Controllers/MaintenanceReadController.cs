using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/maintenance")]
public sealed class MaintenanceReadController(
    IInteractor<GetMaintenanceQuery, PagedResult<MaintenanceListItem>> list) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<MaintenanceListItem>> List(
        [FromQuery] int? itemId,
        [FromQuery] int? status,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20)
        => await list.ExecuteAsync(new GetMaintenanceQuery(itemId, status, page, pageSize));
}