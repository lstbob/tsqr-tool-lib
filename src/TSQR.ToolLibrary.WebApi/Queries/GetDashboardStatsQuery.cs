using MediatR;
using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

public record GetDashboardStatsQuery : IRequest<DashboardStats>;

public sealed class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStats>
{
    private readonly IDashboardQueries _dashboardQueries;

    public GetDashboardStatsHandler(IDashboardQueries dashboardQueries)
    {
        _dashboardQueries = dashboardQueries;
    }

    public async Task<DashboardStats> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var data = await _dashboardQueries.GetStatsAsync(ct);
        return new DashboardStats(data.TotalTools, data.TotalMembers, data.ActiveLoans, data.UnderMaintenance, data.PendingReservations);
    }
}
