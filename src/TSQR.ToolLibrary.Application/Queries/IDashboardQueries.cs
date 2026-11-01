namespace TSQR.ToolLibrary.Application.Queries;

public record DashboardData(int TotalTools, int TotalMembers, int ActiveLoans, int UnderMaintenance, int PendingReservations);

public interface IDashboardQueries
{
    Task<DashboardData> GetStatsAsync(CancellationToken cancellationToken = default);
}
