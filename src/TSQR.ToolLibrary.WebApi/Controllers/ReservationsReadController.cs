using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/reservations")]
public sealed class ReservationsReadController(
    IInteractor<GetReservationsQuery, PagedResult<ReservationListItem>> list,
    IInteractor<GetReservationByIdQuery, ReservationListItem?> byId) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<ReservationListItem>> List(
        [FromQuery] int? status,
        [FromQuery] int? memberId,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20)
        => await list.ExecuteAsync(new GetReservationsQuery(status, memberId, page, pageSize));

    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationListItem>> Get(int id)
    {
        var result = await byId.ExecuteAsync(new GetReservationByIdQuery(id));
        return result is null ? NotFound() : result;
    }
}