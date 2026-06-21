using TSQR.ToolLibrary.Domain;

namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

public sealed class DashboardQueries : IDashboardQueries
{
    private readonly ISqlUnitOfWork _uow;

    public DashboardQueries(ISqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DashboardData> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var db = _uow.Connection;

        var totalTools = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Tools");
        var totalMembers = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Members");
        var activeLoans = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM InventoryItems WHERE Status = 3");
        var underMaintenance = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM InventoryItems WHERE IsUnderRepair = TRUE");
        var pendingReservations = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Reservations WHERE Status = 1");

        return new DashboardData(totalTools, totalMembers, activeLoans, underMaintenance, pendingReservations);
    }
}
