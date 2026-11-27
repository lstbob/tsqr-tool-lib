namespace TSQR.ToolLibrary.Infrastructure.Persistence;

public class InMemoryLoanRepository
    : InMemoryRepository<Loan, LoanId>,
        ILoanRepository
{
    public Task<IReadOnlyCollection<Loan>> GetActiveLoansByMemberAsync(
        MemberId memberId,
        CancellationToken cancellationToken = default)
    {
        var loans = GetAll()
            .Where(loan => loan.MemberId.Equals(memberId) && loan.Status == LoanStatus.Active)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<Loan>>(loans);
    }

    public Task<Loan?> GetActiveLoanByItemAsync(
        InventoryItemId itemId,
        CancellationToken cancellationToken = default)
    {
        var loan = GetAll()
            .Where(l => l.ItemId.Equals(itemId) && l.Status == LoanStatus.Active)
            .OrderByDescending(l => l.CheckoutDate)
            .FirstOrDefault();

        return Task.FromResult(loan);
    }

    public Task<decimal> GetOutstandingFineBalanceAsync(
        MemberId memberId,
        CancellationToken cancellationToken = default)
    {
        var balance = GetAll()
            .Where(loan => loan.MemberId.Equals(memberId))
            .Sum(loan => loan.FineAccrued);

        return Task.FromResult(balance);
    }
}
