using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class LoanRehydrationTests
{
    private static readonly LoanId LoanId = new(1);
    private static readonly MemberId MemberId = new(2);
    private static readonly InventoryItemId ItemId = new(3);
    private static readonly DateTime CheckoutDate = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Rehydrate_ActiveOverdueLoan_PreservesStateWithoutRaisingEvents()
    {
        var dueDate = CheckoutDate.AddDays(7);

        var loan = Loan.Rehydrate(
            LoanId,
            MemberId,
            CheckoutDate,
            dueDate,
            ItemId,
            LoanStatus.Active,
            returnedDate: null,
            fineAccrued: 3.50m,
            lateFeePerDay: 1m,
            renewalCount: 1,
            communityId: 4);

        Assert.Equal(LoanId, loan.Id);
        Assert.Equal(MemberId, loan.MemberId);
        Assert.Equal(ItemId, loan.ItemId);
        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.Equal(dueDate, loan.DueDate);
        Assert.Equal(3.50m, loan.FineAccrued);
        Assert.Equal(1, loan.RenewalCount);
        Assert.Empty(loan.DomainEvents);
    }

    [Fact]
    public void Rehydrate_ReturnedLoan_PreservesReturnedDateAndFine()
    {
        var dueDate = CheckoutDate.AddDays(7);
        var returnedDate = dueDate.AddDays(2);

        var loan = Loan.Rehydrate(
            LoanId,
            MemberId,
            CheckoutDate,
            dueDate,
            ItemId,
            LoanStatus.Returned,
            returnedDate,
            fineAccrued: 2m,
            lateFeePerDay: 1m,
            renewalCount: 0,
            communityId: 4);

        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Equal(returnedDate, loan.ReturnedDate);
        Assert.Equal(2m, loan.FineAccrued);
        Assert.Empty(loan.DomainEvents);
    }
}
