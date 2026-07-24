using Dapper;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Queries;

// ============================================================
// Members
// ============================================================
public record GetMembersQuery(string? Search, int? Status, int Page, int PageSize);

public sealed class GetMembersHandler(ISqlUnitOfWork uow)
    : IInteractor<GetMembersQuery, PagedResult<MemberListItem>>
{
    public async Task<PagedResult<MemberListItem>> ExecuteAsync(GetMembersQuery q, CancellationToken ct = default)
    {
        const string baseSql = """
            SELECT m.Id, m.FirstName, m.MiddleName, m.LastName,
                   (m.LastName || ', ' || m.FirstName) AS FullName,
                   m.Age, m.Email, m.PhoneNumber, m.Status, m.IsVerified,
                   m.MembershipType, m.StartDate, m.EndDate
            FROM Members m
            WHERE 1=1
            """;
        var where = new List<string>();
        var args = new DynamicParameters();
        args.Add("Offset", (q.Page - 1) * q.PageSize);
        args.Add("PageSize", q.PageSize);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var like = $"%{q.Search.ToLowerInvariant()}%";
            where.Add("(LOWER(m.FirstName) LIKE @Search OR LOWER(m.LastName) LIKE @Search OR LOWER(m.Email) LIKE @Search)");
            args.Add("Search", like);
        }
        if (q.Status is > 0)
        {
            where.Add("m.Status = @Status");
            args.Add("Status", q.Status);
        }
        var whereSql = where.Count > 0 ? " AND " + string.Join(" AND ", where) : "";
        var countSql = $"SELECT COUNT(*) FROM Members m WHERE 1=1{whereSql}";
        var total = await uow.Connection.ExecuteScalarAsync<int>(countSql, args);
        var sql = $"{baseSql}{whereSql} ORDER BY m.LastName, m.FirstName LIMIT @PageSize OFFSET @Offset";
        var rows = await uow.Connection.QueryAsync<MemberRow>(sql, args);
        var items = rows.Select(r => new MemberListItem(
            r.Id, r.FirstName, r.MiddleName, r.LastName, r.FullName, r.Age, r.Email, r.PhoneNumber,
            r.Status, ((TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.MemberStatus)r.Status).ToString(),
            r.IsVerified, r.MembershipType,
            r.MembershipType is int mt ? ((TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.MembershipType)mt).ToString() : null,
            r.StartDate, r.EndDate)).ToList();
        return new PagedResult<MemberListItem>(items, total, q.Page, q.PageSize);
    }

    public sealed record MemberRow(int Id, string FirstName, string MiddleName, string LastName, string FullName,
        int Age, string Email, string PhoneNumber, int Status, bool IsVerified,
        int? MembershipType, DateTime? StartDate, DateTime? EndDate);
}

public record GetMemberByIdQuery(int Id);

public sealed class GetMemberByIdHandler(ISqlUnitOfWork uow)
    : IInteractor<GetMemberByIdQuery, MemberDetail?>
{
    public async Task<MemberDetail?> ExecuteAsync(GetMemberByIdQuery q, CancellationToken ct = default)
    {
        var r = await uow.Connection.QuerySingleOrDefaultAsync<MemberRow>(
            "SELECT Id, FirstName, MiddleName, LastName, (LastName || ', ' || FirstName) AS FullName, Age, Address, Email, PhoneNumber, Status, IsVerified, VerifiedByAdminId, VerificationDate, MembershipType, StartDate, EndDate FROM Members WHERE Id = @Id",
            new { q.Id });
        if (r is null)
            return null;
        return new MemberDetail(
            r.Id, r.FirstName, r.MiddleName, r.LastName, r.FullName, r.Age, r.Address, r.Email, r.PhoneNumber,
            r.Status, ((TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.MemberStatus)r.Status).ToString(),
            r.IsVerified, r.VerifiedByAdminId, r.VerificationDate, r.MembershipType,
            r.MembershipType is int mt ? ((TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.MembershipType)mt).ToString() : null,
            r.StartDate, r.EndDate);
    }

    public sealed record MemberRow(int Id, string FirstName, string MiddleName, string LastName, string FullName,
        int Age, string Address, string Email, string PhoneNumber, int Status, bool IsVerified,
        int? VerifiedByAdminId, DateTime? VerificationDate, int? MembershipType, DateTime? StartDate, DateTime? EndDate);
}

// ============================================================
// Reservations
// ============================================================
public record GetReservationsQuery(int? Status, int? MemberId, int Page, int PageSize);

public sealed class GetReservationsHandler(ISqlUnitOfWork uow)
    : IInteractor<GetReservationsQuery, PagedResult<ReservationListItem>>
{
    public async Task<PagedResult<ReservationListItem>> ExecuteAsync(GetReservationsQuery q, CancellationToken ct = default)
    {
        var where = new List<string>();
        var args = new DynamicParameters();
        args.Add("Offset", (q.Page - 1) * q.PageSize);
        args.Add("PageSize", q.PageSize);
        if (q.Status is > 0) { where.Add("r.Status = @Status"); args.Add("Status", q.Status); }
        if (q.MemberId is > 0) { where.Add("r.MemberId = @MemberId"); args.Add("MemberId", q.MemberId); }
        var whereSql = where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "";

        var countSql = $"SELECT COUNT(*) FROM Reservations r{whereSql}";
        var total = await uow.Connection.ExecuteScalarAsync<int>(countSql, args);

        var sql = $"""
            SELECT r.Id, r.ItemId, i.SerialNumber AS ItemSerialNumber,
                   t.Model AS ToolModel, r.MemberId,
                   (m.LastName || ', ' || m.FirstName) AS MemberName,
                   r.ReservationDate, r.ExpiryDate, r.Status, r.IsConfirmed, r.QueuePosition
            FROM Reservations r
            JOIN InventoryItems i ON i.Id = r.ItemId
            JOIN Tools t ON t.Id = i.ToolId
            JOIN Members m ON m.Id = r.MemberId
            {whereSql}
            ORDER BY r.ReservationDate DESC
            LIMIT @PageSize OFFSET @Offset
            """;
        var rows = await uow.Connection.QueryAsync<Row>(sql, args);
        var items = rows.Select(r => new ReservationListItem(
            r.Id, r.ItemId, r.ItemSerialNumber ?? "", r.ToolModel ?? "", r.MemberId, r.MemberName ?? "",
            r.ReservationDate, r.ExpiryDate, r.Status,
            ((TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.ReservationStatus)r.Status).ToString(),
            r.IsConfirmed, r.QueuePosition)).ToList();
        return new PagedResult<ReservationListItem>(items, total, q.Page, q.PageSize);
    }

    public sealed record Row(int Id, int ItemId, string? ItemSerialNumber, string? ToolModel, int MemberId,
        string? MemberName, DateTime ReservationDate, DateTime ExpiryDate, int Status, bool IsConfirmed, int QueuePosition);
}

public record GetReservationByIdQuery(int Id);

public sealed class GetReservationByIdHandler(ISqlUnitOfWork uow)
    : IInteractor<GetReservationByIdQuery, ReservationListItem?>
{
    public async Task<ReservationListItem?> ExecuteAsync(GetReservationByIdQuery q, CancellationToken ct = default)
    {
        const string sql = """
            SELECT r.Id, r.ItemId, i.SerialNumber AS ItemSerialNumber,
                   t.Model AS ToolModel, r.MemberId,
                   (m.LastName || ', ' || m.FirstName) AS MemberName,
                   r.ReservationDate, r.ExpiryDate, r.Status, r.IsConfirmed, r.QueuePosition
            FROM Reservations r
            JOIN InventoryItems i ON i.Id = r.ItemId
            JOIN Tools t ON t.Id = i.ToolId
            JOIN Members m ON m.Id = r.MemberId
            WHERE r.Id = @Id
            """;
        var r = await uow.Connection.QuerySingleOrDefaultAsync<Row>(sql, new { q.Id });
        if (r is null)
            return null;
        return new ReservationListItem(
            r.Id, r.ItemId, r.ItemSerialNumber ?? "", r.ToolModel ?? "", r.MemberId, r.MemberName ?? "",
            r.ReservationDate, r.ExpiryDate, r.Status,
            ((TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.ReservationStatus)r.Status).ToString(),
            r.IsConfirmed, r.QueuePosition);
    }

    public sealed record Row(int Id, int ItemId, string? ItemSerialNumber, string? ToolModel, int MemberId,
        string? MemberName, DateTime ReservationDate, DateTime ExpiryDate, int Status, bool IsConfirmed, int QueuePosition);
}

// ============================================================
// Inventory
// ============================================================
public record GetInventoryQuery(int? ToolId, int? Status, int Page, int PageSize);

public sealed class GetInventoryHandler(ISqlUnitOfWork uow)
    : IInteractor<GetInventoryQuery, PagedResult<InventoryListItem>>
{
    public async Task<PagedResult<InventoryListItem>> ExecuteAsync(GetInventoryQuery q, CancellationToken ct = default)
    {
        var where = new List<string>();
        var args = new DynamicParameters();
        args.Add("Offset", (q.Page - 1) * q.PageSize);
        args.Add("PageSize", q.PageSize);
        if (q.ToolId is > 0) { where.Add("i.ToolId = @ToolId"); args.Add("ToolId", q.ToolId); }
        if (q.Status is > 0) { where.Add("i.Status = @Status"); args.Add("Status", q.Status); }
        var whereSql = where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "";

        var total = await uow.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM InventoryItems i{whereSql}", args);

        const string fixedSql = """
            SELECT i.Id, i.ToolId, t.Model AS ToolModel,
                   t.ToolType AS ToolTypeId,
                   CASE t.ToolType WHEN 1 THEN 'Hand Tool' WHEN 2 THEN 'Power Tool' WHEN 3 THEN 'Gardening Tool'
                       WHEN 4 THEN 'Construction Tool' WHEN 5 THEN 'Specialty Tool' ELSE 'Other' END AS ToolTypeName,
                   i.OriginalOwnerId,
                   (om.LastName || ', ' || om.FirstName) AS OriginalOwnerName,
                   i.InitialAcquisitionDate, i.SerialNumber, i.Status, i.Condition,
                   i.CurrentHolderId,
                   (cm.LastName || ', ' || cm.FirstName) AS CurrentHolderName,
                   i.LastBorrowedDate, i.LoanCount, i.IsUnderRepair
            FROM InventoryItems i
            JOIN Tools t ON t.Id = i.ToolId
            LEFT JOIN Members om ON om.Id = i.OriginalOwnerId
            LEFT JOIN Members cm ON cm.Id = i.CurrentHolderId
            """;
        var pagedSql = $"{fixedSql}{whereSql} ORDER BY i.Id LIMIT @PageSize OFFSET @Offset";

        var rows = await uow.Connection.QueryAsync<Row>(pagedSql, args);
        var items = rows.Select(r => new InventoryListItem(
            r.Id, r.ToolId, r.ToolModel ?? "", r.ToolTypeName ?? "", r.OriginalOwnerId, r.OriginalOwnerName,
            r.InitialAcquisitionDate, r.SerialNumber ?? "", r.Status,
            ((TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate.ItemStatus)r.Status).ToString(),
            r.Condition,
            ((TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate.Condition)r.Condition).ToString(),
            r.CurrentHolderId, r.CurrentHolderName, r.LastBorrowedDate, r.LoanCount, r.IsUnderRepair)).ToList();

        return new PagedResult<InventoryListItem>(items, total, q.Page, q.PageSize);
    }

    public sealed record Row(int Id, int ToolId, string? ToolModel, int ToolTypeId, string? ToolTypeName,
        int? OriginalOwnerId, string? OriginalOwnerName, DateTime InitialAcquisitionDate, string? SerialNumber,
        int Status, int Condition, int? CurrentHolderId, string? CurrentHolderName,
        DateTime? LastBorrowedDate, int LoanCount, bool IsUnderRepair);
}

public record GetInventoryByIdQuery(int Id);

public sealed class GetInventoryByIdHandler(ISqlUnitOfWork uow)
    : IInteractor<GetInventoryByIdQuery, InventoryListItem?>
{
    public async Task<InventoryListItem?> ExecuteAsync(GetInventoryByIdQuery q, CancellationToken ct = default)
    {
        const string sql = """
            SELECT i.Id, i.ToolId, t.Model AS ToolModel,
                   t.ToolType AS ToolTypeId,
                   CASE t.ToolType WHEN 1 THEN 'Hand Tool' WHEN 2 THEN 'Power Tool' WHEN 3 THEN 'Gardening Tool'
                       WHEN 4 THEN 'Construction Tool' WHEN 5 THEN 'Specialty Tool' ELSE 'Other' END AS ToolTypeName,
                   i.OriginalOwnerId,
                   (om.LastName || ', ' || om.FirstName) AS OriginalOwnerName,
                   i.InitialAcquisitionDate, i.SerialNumber, i.Status, i.Condition,
                   i.CurrentHolderId,
                   (cm.LastName || ', ' || cm.FirstName) AS CurrentHolderName,
                   i.LastBorrowedDate, i.LoanCount, i.IsUnderRepair
            FROM InventoryItems i
            JOIN Tools t ON t.Id = i.ToolId
            LEFT JOIN Members om ON om.Id = i.OriginalOwnerId
            LEFT JOIN Members cm ON cm.Id = i.CurrentHolderId
            WHERE i.Id = @Id
            """;
        var r = await uow.Connection.QuerySingleOrDefaultAsync<Row>(sql, new { q.Id });
        if (r is null)
            return null;
        return new InventoryListItem(
            r.Id, r.ToolId, r.ToolModel ?? "", r.ToolTypeName ?? "", r.OriginalOwnerId, r.OriginalOwnerName,
            r.InitialAcquisitionDate, r.SerialNumber ?? "", r.Status,
            ((TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate.ItemStatus)r.Status).ToString(),
            r.Condition,
            ((TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate.Condition)r.Condition).ToString(),
            r.CurrentHolderId, r.CurrentHolderName, r.LastBorrowedDate, r.LoanCount, r.IsUnderRepair);
    }

    public sealed record Row(int Id, int ToolId, string? ToolModel, int ToolTypeId, string? ToolTypeName,
        int? OriginalOwnerId, string? OriginalOwnerName, DateTime InitialAcquisitionDate, string? SerialNumber,
        int Status, int Condition, int? CurrentHolderId, string? CurrentHolderName,
        DateTime? LastBorrowedDate, int LoanCount, bool IsUnderRepair);
}