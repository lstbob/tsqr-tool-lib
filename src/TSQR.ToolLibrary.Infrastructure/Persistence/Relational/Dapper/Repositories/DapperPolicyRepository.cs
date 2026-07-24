using TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Infrastructure.Persistence.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class DapperPolicyRepository(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<Policy> mapping,
    ISqlDialect dialect)
    : SqlRepository<Policy, PolicyId>(uow, mapping, dialect), IPolicyRepository
{
    public async Task<Policy?> GetByToolTypeAsync(
        ToolType toolType,
        LocationId? locationId,
        CancellationToken cancellationToken = default
    )
    {
        var rows = await Database.QueryAsync<PolicyRow>(
            @"SELECT Id, ToolType, LocationId, Name, Description,
                     LateFeePerDay, MaxLoanDurationDays, MaxRenewalCount,
                     MaxLoanReservationDays
              FROM Policies
              WHERE ToolType = @ToolType
                AND (LocationId IS @LocationId OR LocationId IS NULL)
              ORDER BY LocationId NULLS LAST
              LIMIT 1",
            new { ToolType = toolType, LocationId = locationId?.Value }
        );

        var row = rows.FirstOrDefault();
        if (row is null)
            return null;

        var result = Policy.Create(
            row.Id,
            row.ToolType,
            row.LocationId is null ? null : new LocationId(row.LocationId.Value),
            row.Name,
            row.Description,
            row.LateFeePerDay,
            row.MaxLoanDurationDays,
            row.MaxRenewalCount,
            row.MaxLoanReservationDays);

        return result.IsSuccess ? result.Value : null;
    }

    private sealed record PolicyRow(
        PolicyId Id,
        ToolType ToolType,
        int? LocationId,
        string Name,
        string Description,
        decimal LateFeePerDay,
        int MaxLoanDurationDays,
        int MaxRenewalCount,
        int MaxLoanReservationDays);
}
