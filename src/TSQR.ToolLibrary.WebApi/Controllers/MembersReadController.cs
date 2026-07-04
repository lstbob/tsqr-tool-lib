using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/members")]
public sealed class MembersReadController(
    IInteractor<GetMembersQuery, PagedResult<MemberListItem>> list,
    IInteractor<GetMemberByIdQuery, MemberDetail?> byId) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<MemberListItem>> List(
        [FromQuery] string? search,
        [FromQuery] int? status,
        [FromQuery][Range(1, int.MaxValue)] int page = 1,
        [FromQuery][Range(1, 100)] int pageSize = 20)
        => await list.ExecuteAsync(new GetMembersQuery(search, status, page, pageSize));

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDetail>> Get(int id)
    {
        var result = await byId.ExecuteAsync(new GetMemberByIdQuery(id));
        return result is null ? NotFound() : result;
    }
}