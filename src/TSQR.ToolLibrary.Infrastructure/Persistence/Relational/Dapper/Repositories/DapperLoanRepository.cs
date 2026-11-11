using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class DapperLoanRepository(
    ISqlUnitOfWork uow,
    ISqlEntityMapping<Loan, LoanId> mapping,
    ISqlDialect dialect,
    IDomainEventCollector eventCollector)
    : SqlRepository<Loan, LoanId>(uow, mapping, dialect, eventCollector), ILoanRepository
{
    public async Task<IReadOnlyCollection<Loan>> GetActiveLoansByMemberAsync(
        MemberId memberId,
        CancellationToken cancellationToken = default)
    {
        var rows = await Database.QueryAsync<LoanRow>(
            @"SELECT Id, MemberId, CheckoutDate, DueDate, ItemId,
                     Status, ReturnedDate, FineAccrued, LateFeePerDay, RenewalCount, CommunityId,
                     xmin::text::bigint AS RowVersion
               FROM Loans
               WHERE MemberId = @MemberId AND Status = @Status",
            new { MemberId = memberId.Value, Status = (int)LoanStatus.Active });

        return rows.Select(row =>
            {
                var loan = Loan.Rehydrate(
                    row.Id,
                    row.MemberId,
                    row.CheckoutDate,
                    row.DueDate,
                    row.ItemId,
                    row.Status,
                    row.ReturnedDate,
                    row.FineAccrued,
                    row.LateFeePerDay,
                    row.RenewalCount,
                    row.CommunityId);

                loan.SetRowVersion(row.RowVersion);
                return loan;
            })
            .ToList();
    }

    public Task<decimal> GetOutstandingFineBalanceAsync(
        MemberId memberId,
        CancellationToken cancellationToken = default)
    {
        // Payment/waiver tracking is not modelled yet, so every accrued fine is
        // treated as outstanding until that capability is added.
        return Database.ExecuteScalarAsync<decimal>(
            @"SELECT COALESCE(SUM(FineAccrued), 0)
              FROM Loans
              WHERE MemberId = @MemberId AND FineAccrued > 0",
            new { MemberId = memberId.Value });
    }
}
