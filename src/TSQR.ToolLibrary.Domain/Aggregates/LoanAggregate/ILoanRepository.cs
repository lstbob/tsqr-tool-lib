namespace TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;

public interface ILoanRepository : IRepository<Loan, LoanId>
{
    Task<IReadOnlyCollection<Loan>> GetActiveLoansByMemberAsync(
        MemberId memberId,
        CancellationToken cancellationToken = default);

    Task<Loan?> GetActiveLoanByItemAsync(InventoryItemId itemId, CancellationToken cancellationToken = default);

    Task<decimal> GetOutstandingFineBalanceAsync(
        MemberId memberId,
        CancellationToken cancellationToken = default);
}
