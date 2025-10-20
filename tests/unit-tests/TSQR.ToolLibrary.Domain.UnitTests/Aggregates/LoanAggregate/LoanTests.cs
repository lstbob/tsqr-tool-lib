using System;
using TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using Xunit;

namespace TSQR.ToolLibrary.Domain.UnitTests.Aggregates.LoanAggregate;

public class LoanTests
{
    [Fact]
    public void CreateAndEndLoan_Returns_ReturnedStatus()
    {
        var memberId = new MemberId(1);
        var itemId = new InventoryItemId(2);
        var checkout = DateTime.UtcNow;
        // due in the future
        var due = DateTime.UtcNow.AddMinutes(2);

        var loan = Loan.Create(memberId, checkout, LoanStatus.Active, due, itemId);

        // end before due, but still in the future to satisfy validation
        var expectedEnd = DateTime.UtcNow.AddMinutes(1);
        loan.EndLoan(expectedEnd);

        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.NotEqual(default, loan.ReturnedDate);
    }

    [Fact]
    public void EndLoan_SetsOverdue_WhenPastDue()
    {
        var memberId = new MemberId(1);
        var itemId = new InventoryItemId(2);
        var checkout = DateTime.UtcNow;
        // set due a bit in the future
        var due = DateTime.UtcNow.AddMinutes(1);

        var loan = Loan.Create(memberId, checkout, LoanStatus.Active, due, itemId);

        // end after due
        var expectedEnd = DateTime.UtcNow.AddMinutes(2);
        loan.EndLoan(expectedEnd);

        Assert.Equal(LoanStatus.Overdue, loan.Status);
    }
}
