using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

internal sealed record LoanRow
{
    public LoanId Id { get; init; }
    public MemberId MemberId { get; init; }
    public DateTime CheckoutDate { get; init; }
    public DateTime DueDate { get; init; }
    public InventoryItemId ItemId { get; init; }
    public LoanStatus Status { get; init; }
    public DateTime? ReturnedDate { get; init; }
    public decimal FineAccrued { get; init; }
    public decimal LateFeePerDay { get; init; }
    public int RenewalCount { get; init; }
    public int CommunityId { get; init; }
}

internal sealed record LoanInsertDto(
    int MemberId,
    DateTime CheckoutDate,
    DateTime DueDate,
    int ItemId,
    LoanStatus Status,
    decimal FineAccrued,
    decimal LateFeePerDay,
    int RenewalCount,
    int CommunityId);

internal sealed record LoanUpdateDto(
    int Id,
    int MemberId,
    DateTime CheckoutDate,
    DateTime DueDate,
    int ItemId,
    LoanStatus Status,
    DateTime? ReturnedDate,
    decimal FineAccrued,
    decimal LateFeePerDay,
    int RenewalCount,
    int CommunityId);

public sealed class LoanMapping : ISqlEntityMapping<Loan>
{
    public string TableName => "Loans";

    public string InsertSql =>
        @"INSERT INTO Loans (MemberId, CheckoutDate, DueDate, ItemId, Status, FineAccrued, LateFeePerDay, RenewalCount, CommunityId)
          VALUES (@MemberId, @CheckoutDate, @DueDate, @ItemId, @Status, @FineAccrued, @LateFeePerDay, @RenewalCount, @CommunityId)
";

    public string UpdateSql =>
        @"UPDATE Loans
          SET MemberId = @MemberId, CheckoutDate = @CheckoutDate,
              DueDate = @DueDate, ItemId = @ItemId,
              Status = @Status, ReturnedDate = @ReturnedDate,
              FineAccrued = @FineAccrued, LateFeePerDay = @LateFeePerDay,
              RenewalCount = @RenewalCount, CommunityId = @CommunityId
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM Loans WHERE Id = @Id";

    public async Task<Loan?> GetByIdAsync(ISqlConnection db, int id)
    {
        var row = await db.QuerySingleOrDefaultAsync<LoanRow>(
            @"SELECT Id, MemberId, CheckoutDate, DueDate, ItemId,
                     Status, ReturnedDate, FineAccrued, LateFeePerDay, RenewalCount, CommunityId
               FROM Loans WHERE Id = @Id", new { Id = id });

        if (row is null)
            return null;

        var result = Loan.Create(
            row.Id,
            row.MemberId,
            row.CheckoutDate,
            row.DueDate,
            row.ItemId,
            row.Status,
            row.LateFeePerDay,
            row.RenewalCount,
            row.CommunityId);
        return result.IsSuccess ? result.Value : null;
    }

    public object ToInsertParameters(Loan entity) => new LoanInsertDto(
        entity.MemberId.Value,
        entity.CheckoutDate,
        entity.DueDate,
        entity.ItemId.Value,
        entity.Status,
        entity.FineAccrued,
        entity.LateFeePerDay,
        entity.RenewalCount,
        entity.CommunityId);

    public object ToUpdateParameters(Loan entity) => new LoanUpdateDto(
        entity.Id.Value,
        entity.MemberId.Value,
        entity.CheckoutDate,
        entity.DueDate,
        entity.ItemId.Value,
        entity.Status,
        entity.ReturnedDate == default ? null : entity.ReturnedDate,
        entity.FineAccrued,
        entity.LateFeePerDay,
        entity.RenewalCount,
        entity.CommunityId);
}