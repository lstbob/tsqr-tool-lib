using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;
using TSQR.ToolLibrary.WebApi.Queries;

namespace TSQR.ToolLibrary.WebApi.Controllers;

[ApiController]
[AllowAnonymous] // public read-only dashboard stats (consumed unauthenticated by the UI home page)
[Route("api/dashboard")]
public sealed class DashboardController(
    IInteractor<GetDashboardStatsQuery, DashboardStats> getStats
) : ControllerBase
{
    private readonly IInteractor<GetDashboardStatsQuery, DashboardStats> _getStats = getStats;

    [HttpGet("stats")]
    public async Task<DashboardStats> GetStats()
    {
        return await _getStats.ExecuteAsync(new GetDashboardStatsQuery());
    }
}
