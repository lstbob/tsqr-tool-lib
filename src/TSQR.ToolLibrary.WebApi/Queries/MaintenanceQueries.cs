using Dapper;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

// ============================================================
// Maintenance records
// ============================================================
public record GetMaintenanceQuery(int? ItemId, int? Status, int Page, int PageSize);

public sealed class GetMaintenanceHandler(ISqlUnitOfWork uow)
    : IInteractor<GetMaintenanceQuery, PagedResult<MaintenanceListItem>>
{
    public async Task<PagedResult<MaintenanceListItem>> ExecuteAsync(GetMaintenanceQuery q, CancellationToken ct = default)
    {
        var where = new List<string>();
        var args = new DynamicParameters();
        args.Add("Offset", (q.Page - 1) * q.PageSize);
        args.Add("PageSize", q.PageSize);
        if (q.ItemId is > 0) { where.Add("mr.ItemId = @ItemId"); args.Add("ItemId", q.ItemId); }
        if (q.Status is > 0) { where.Add("mr.Status = @Status"); args.Add("Status", q.Status); }
        var whereSql = where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "";

        var total = await uow.Connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM MaintenanceRecords mr{whereSql}", args);

        const string sql = """
            SELECT mr.Id, mr.ItemId, i.SerialNumber AS ItemSerialNumber,
                   t.Model AS ToolModel, mr.ReportedById,
                   (rm.LastName || ', ' || rm.FirstName) AS ReportedByName,
                   mr.ReportedDate, mr.Description, mr.Status,
                   mr.CompletedById,
                   (cm.LastName || ', ' || cm.FirstName) AS CompletedByName,
                   mr.CompletedDate, mr.ResultingCondition
            FROM MaintenanceRecords mr
            JOIN InventoryItems i ON i.Id = mr.ItemId
            JOIN Tools t ON t.Id = i.ToolId
            LEFT JOIN Members rm ON rm.Id = mr.ReportedById
            LEFT JOIN Members cm ON cm.Id = mr.CompletedById
            """;
        var pagedSql = $"{sql}{whereSql} ORDER BY mr.ReportedDate DESC LIMIT @PageSize OFFSET @Offset";

        var rows = await uow.Connection.QueryAsync<Row>(pagedSql, args);
        var items = rows.Select(r => new MaintenanceListItem(
            r.Id, r.ItemId, r.ItemSerialNumber ?? "", r.ToolModel ?? "", r.ReportedById, r.ReportedByName ?? "",
            r.ReportedDate, r.Description ?? "", r.Status,
            ((TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate.MaintenanceStatus)r.Status).ToString(),
            r.CompletedById, r.CompletedByName, r.CompletedDate, r.ResultingCondition,
            r.ResultingCondition is int c
                ? ((TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate.Condition)c).ToString()
                : null)).ToList();

        return new PagedResult<MaintenanceListItem>(items, total, q.Page, q.PageSize);
    }

    public sealed record Row(int Id, int ItemId, string? ItemSerialNumber, string? ToolModel, int ReportedById,
        string? ReportedByName, DateTime ReportedDate, string? Description, int Status,
        int? CompletedById, string? CompletedByName, DateTime? CompletedDate, int? ResultingCondition);
}