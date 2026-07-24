using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

internal sealed record PolicyRow
{
    public PolicyId Id { get; init; }
    public ToolType ToolType { get; init; }
    public int? LocationId { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal LateFeePerDay { get; init; }
    public int MaxLoanDurationDays { get; init; }
    public int MaxRenewalCount { get; init; }
    public int MaxLoanReservationDays { get; init; }
}

internal sealed record PolicyInsertDto(
    ToolType ToolType,
    int? LocationId,
    string Name,
    string Description,
    decimal LateFeePerDay,
    int MaxLoanDurationDays,
    int MaxRenewalCount,
    int MaxLoanReservationDays);

internal sealed record PolicyUpdateDto(
    int Id,
    ToolType ToolType,
    int? LocationId,
    string Name,
    string Description,
    decimal LateFeePerDay,
    int MaxLoanDurationDays,
    int MaxRenewalCount,
    int MaxLoanReservationDays);

public sealed class PolicyMapping : ISqlEntityMapping<Policy>
{
    public string TableName => "Policies";

    public string InsertSql =>
        @"INSERT INTO Policies (ToolType, LocationId, Name, Description, LateFeePerDay,
                MaxLoanDurationDays, MaxRenewalCount, MaxLoanReservationDays)
          VALUES (@ToolType, @LocationId, @Name, @Description, @LateFeePerDay,
                  @MaxLoanDurationDays, @MaxRenewalCount, @MaxLoanReservationDays)
";

    public string UpdateSql =>
        @"UPDATE Policies
          SET ToolType = @ToolType, LocationId = @LocationId,
              Name = @Name, Description = @Description,
              LateFeePerDay = @LateFeePerDay,
              MaxLoanDurationDays = @MaxLoanDurationDays,
              MaxRenewalCount = @MaxRenewalCount,
              MaxLoanReservationDays = @MaxLoanReservationDays
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM Policies WHERE Id = @Id";

    public async Task<Policy?> GetByIdAsync(ISqlConnection db, int id)
    {
        var row = await db.QuerySingleOrDefaultAsync<PolicyRow>(
            @"SELECT Id, ToolType, LocationId, Name, Description,
                     LateFeePerDay, MaxLoanDurationDays, MaxRenewalCount,
                     MaxLoanReservationDays
              FROM Policies WHERE Id = @Id", new { Id = id });

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

    public object ToInsertParameters(Policy entity) => new PolicyInsertDto(
        entity.ToolType,
        entity.LocationId?.Value,
        entity.Name,
        entity.Description,
        entity.LateFeePerDay,
        entity.MaxLoanDurationDays,
        entity.MaxRenewalCount,
        entity.MaxLoanReservationDays);

    public object ToUpdateParameters(Policy entity) => new PolicyUpdateDto(
        entity.Id.Value,
        entity.ToolType,
        entity.LocationId?.Value,
        entity.Name,
        entity.Description,
        entity.LateFeePerDay,
        entity.MaxLoanDurationDays,
        entity.MaxRenewalCount,
        entity.MaxLoanReservationDays);
}