using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

/// <summary>
/// Read-side view of loans. A "loan" in the tool-library model is an inventory
/// item currently in <c>Loaned</c> status (ItemStatus=3); there is no separate
/// Loans table. The <c>ItemId</c> returned here corresponds to the loaned item.
/// </summary>
[ApiController]
[Authorize]
[Route("api/loans")]
public sealed class LoansReadController(
    IInteractor<GetInventoryQuery, PagedResult<InventoryListItem>> list,
    IInteractor<GetInventoryByIdQuery, InventoryListItem?> byId) : ControllerBase
{
    private const int LoanedStatus = 3;

    [HttpGet]
    public async Task<PagedResult<InventoryListItem>> List(
        [FromQuery] int? memberId,
        [FromQuery] int? toolId,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20)
    {
        // MemberId filter would require a WHERE on CurrentHolderId, which the
        // underlying inventory query doesn't expose today; expose that filtering
        // via the client by post-filtering until a dedicated loan query lands.
        var items = await list.ExecuteAsync(new GetInventoryQuery(toolId, LoanedStatus, page, pageSize));
        if (memberId is > 0 && items.Items.Count > 0)
        {
            var filtered = items.Items.Where(i => i.CurrentHolderId == memberId).ToList();
            return new PagedResult<InventoryListItem>(filtered, filtered.Count, page, pageSize);
        }
        return items;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryListItem>> Get(int id)
    {
        var item = await byId.ExecuteAsync(new GetInventoryByIdQuery(id));
        if (item is null || item.Status != LoanedStatus)
            return NotFound();
        return item;
    }
}