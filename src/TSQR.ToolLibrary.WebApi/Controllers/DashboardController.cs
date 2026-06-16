using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[AllowAnonymous] // public read-only dashboard stats (consumed unauthenticated by the UI home page)
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IInteractor<GetDashboardStatsQuery, DashboardStats> _getStats;

    public DashboardController(IInteractor<GetDashboardStatsQuery, DashboardStats> getStats)
    {
        _getStats = getStats;
    }

    [HttpGet("stats")]
    public async Task<DashboardStats> GetStats()
    {
        return await _getStats.ExecuteAsync(new GetDashboardStatsQuery());
    }
}
