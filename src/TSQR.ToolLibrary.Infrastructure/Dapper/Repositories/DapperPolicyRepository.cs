using TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

/// <summary>
/// Dapper implementation of <see cref="IPolicyRepository"/>. Lookup is by
/// (ToolType, LocationId?); when no location-specific policy exists, falls
/// back to the global (LocationId=null) policy for the tool type.
/// </summary>
public sealed class DapperPolicyRepository(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<Policy> mapping
) : SqlRepository<Policy, PolicyId>(uow, mapping), IPolicyRepository
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
        if (row is null) return null;

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