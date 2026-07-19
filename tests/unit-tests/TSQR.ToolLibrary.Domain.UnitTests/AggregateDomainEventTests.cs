using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MaintenanceAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Domain.UnitTests;

/// <summary>
/// Phase 1 tests: each aggregate behavior that should raise a domain event
/// does so with the correct event type and content. Guards the domain
/// contract that the <see cref="Application.DomainEventOrchestrator"/> relies
/// on: events originate INSIDE aggregate methods (never from the application
/// layer), so the orchestrator only dispatches what the aggregate itself
/// raised.
/// </summary>
public class AggregateDomainEventTests
{
    [Fact]
    public void Member_Verify_RaisesMemberVerifiedEvent()
    {
        var (member, admin) = MakeMemberAndAdmin();

        var result = member.Verify(admin.Id);

        Assert.True(result.IsSuccess);
        var ev = Assert.Single(member.DomainEvents);
        var verified = Assert.IsType<MemberVerifiedEvent>(ev);
        Assert.Equal(member.Id, verified.MemberId);
        Assert.Equal(admin.Id, verified.VerifiedByAdminId);
        Assert.True(member.IsVerified);
    }

    [Fact]
    public void InventoryItem_Loan_RaisesItemLoanedDomainEvent()
    {
        var item = MakeAvailableItem();

        var borrower = new MemberId(2);
        var result = item.Loan(borrower);

        Assert.True(result.IsSuccess);
        var ev = Assert.Single(item.DomainEvents);
        var loaned = Assert.IsType<ItemLoanedDomainEvent>(ev);
        Assert.Equal(item.Id, loaned.ItemId);
        Assert.Equal(borrower, loaned.BorrowerId);
        Assert.Equal(ItemStatus.Loaned, item.Status);
        Assert.Equal(borrower, item.CurrentHolderId);
    }

    [Fact]
    public void InventoryItem_Return_RaisesToolReturnedEvent()
    {
        var borrower = new MemberId(2);
        var item = MakeLoanedItem(borrower);

        var result = item.Return(Condition.Good);

        Assert.True(result.IsSuccess);
        var ev = Assert.Single(item.DomainEvents);
        var returned = Assert.IsType<ToolReturnedEvent>(ev);
        Assert.Equal(item.Id, returned.ItemId);
        Assert.Equal(borrower, returned.ReturnedByMemberId);
        Assert.Equal(Condition.Good, returned.ReturnedCondition);
    }

    [Fact]
    public void InventoryItem_MarkForRepair_RaisesToolMarkedForRepairEvent()
    {
        var item = MakeAvailableItem();
        var reporter = new MemberId(7);

        var result = item.MarkForRepair(reporter, "broken handle");

        Assert.True(result.IsSuccess);
        var ev = Assert.Single(item.DomainEvents);
        var repair = Assert.IsType<ToolMarkedForRepairEvent>(ev);
        Assert.Equal(item.Id, repair.ItemId);
        Assert.Equal(reporter, repair.ReportedByMemberId);
        Assert.Equal("broken handle", repair.Description);
    }

    [Fact]
    public void Reservation_Cancel_RaisesReservationCancelledEvent()
    {
        var reservation = MakePendingReservation();

        var result = reservation.Cancel();

        Assert.True(result.IsSuccess);
        var ev = Assert.Single(reservation.DomainEvents);
        var cancelled = Assert.IsType<ReservationCancelledEvent>(ev);
        Assert.Equal(reservation.Id, cancelled.ReservationId);
        Assert.Equal(reservation.ItemId, cancelled.ItemId);
        Assert.Equal(reservation.MemberId, cancelled.MemberId);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Reservation_ConfirmPickup_RaisesReservationConfirmedEvent()
    {
        var reservation = MakePendingReservation();

        var result = reservation.ConfirmPickup();

        Assert.True(result.IsSuccess);
        // ConfirmPickup fans out two cross-aggregate signals: the canonical
        // ReservationConfirmedEvent and the PickupReminderEvent that drives the
        // InventoryItem hold side effect in PickupReminderHandler.
        Assert.Equal(2, reservation.DomainEvents.Count);
        var confirmed = Assert.IsType<ReservationConfirmedEvent>(
            reservation.DomainEvents.Single(e => e is ReservationConfirmedEvent));
        Assert.Equal(reservation.Id, confirmed.ReservationId);
        Assert.Equal(reservation.ItemId, confirmed.ItemId);
        Assert.Equal(reservation.MemberId, confirmed.MemberId);
        var reminder = Assert.IsType<PickupReminderEvent>(
            reservation.DomainEvents.Single(e => e is PickupReminderEvent));
        Assert.Equal(reservation.Id, reminder.ReservationId);
        Assert.Equal(reservation.ItemId, reminder.ItemId);
        Assert.Equal(reservation.MemberId, reminder.MemberId);
        Assert.Equal(reservation.ReservationDate, reminder.PickupDate);
        Assert.True(reservation.IsConfirmed);
    }

    [Fact]
    public void Reservation_NotifyNextInLine_RaisesNextInLineNotificationEvent()
    {
        var reservation = MakePendingReservation();

        reservation.NotifyNextInLine("the tool is available");

        var ev = Assert.Single(reservation.DomainEvents);
        var next = Assert.IsType<NextInLineNotificationEvent>(ev);
        Assert.Equal(reservation.Id, next.ReservationId);
        Assert.Equal(reservation.ItemId, next.ItemId);
        Assert.Equal(reservation.MemberId, next.NextMemberId);
        Assert.Equal("the tool is available", next.Reason);
    }

    [Fact]
    public void Loan_EndLoan_WhenOverdue_RaisesLoanOverdueDomainEvent_AndAccruesFine()
    {
        var itemId = new InventoryItemId(42);
        var borrower = new MemberId(7);
        var now = DateTime.UtcNow;
        var checkout = now.AddDays(-10);
        var due = now.AddDays(7);
        var loanResult = Loan.Create(new LoanId(1), borrower, checkout, due, itemId, LoanStatus.Active);
        Assert.True(loanResult.IsSuccess);
        var loan = loanResult.Value;

        var endResult = loan.EndLoan(now.AddDays(10));

        Assert.True(endResult.IsSuccess);
        var ev = Assert.Single(loan.DomainEvents);
        var overdue = Assert.IsType<LoanOverdueDomainEvent>(ev);
        Assert.Equal(loan.Id, overdue.LoanId);
        Assert.Equal(itemId, overdue.ItemId);
        Assert.Equal(LoanStatus.Overdue, loan.Status);
        Assert.True(loan.FineAccrued > 0);
    }

    [Fact]
    public void Loan_EndLoan_WhenNotOverdue_DoesNotRaiseEvent_AndMarksReturned()
    {
        var itemId = new InventoryItemId(42);
        var borrower = new MemberId(7);
        var now = DateTime.UtcNow;
        var checkout = now.AddDays(-1);
        var due = now.AddDays(7);
        var loanResult = Loan.Create(new LoanId(1), borrower, checkout, due, itemId, LoanStatus.Active);
        Assert.True(loanResult.IsSuccess);
        var loan = loanResult.Value;

        var endResult = loan.EndLoan(now.AddMilliseconds(1));

        Assert.True(endResult.IsSuccess);
        Assert.Empty(loan.DomainEvents);
        Assert.Equal(LoanStatus.Returned, loan.Status);
    }

    [Fact]
    public void MaintenanceRecord_Complete_RaisesToolRepairedEvent()
    {
        var recordResult = MaintenanceRecord.Create(
            new InventoryItemId(7),
            new MemberId(1),
            "blade misaligned");
        Assert.True(recordResult.IsSuccess);
        var record = recordResult.Value;

        var startResult = record.StartWork();
        Assert.True(startResult.IsSuccess);

        var completedBy = new MemberId(2);
        var newCondition = Condition.Repaired;
        var completeResult = record.Complete(completedBy, newCondition);

        Assert.True(completeResult.IsSuccess);
        var ev = Assert.Single(record.DomainEvents);
        var repaired = Assert.IsType<ToolRepairedEvent>(ev);
        Assert.Equal(record.ItemId, repaired.ItemId);
        Assert.Equal(completedBy, repaired.RepairedByMemberId);
        Assert.Equal(newCondition, repaired.NewCondition);
    }

    // ---- Fixtures ----

    private static (Member member, Member admin) MakeMemberAndAdmin()
    {
        var member = Member.Create(
            new MemberId(1), "Alice", "", "Smith", 30, "1 Main St",
            "alice@example.com", "555-0100", MemberStatus.Active,
            isVerified: false, verifiedByAdminId: null, verificationDate: null);

        var admin = Member.Create(
            new MemberId(2), "Bob", "", "Admin", 40, "2 Main St",
            "bob@example.com", "555-0200", MemberStatus.Active,
            isVerified: false, verifiedByAdminId: null, verificationDate: null);

        return (member, admin);
    }

    private static InventoryItem MakeAvailableItem() =>
        InventoryItem.Create(
            new InventoryItemId(1),
            new ToolId(1),
            new MemberId(1),
            DateTime.UtcNow.AddDays(-1),
            "SN-001",
            ItemStatus.Available,
            Condition.New,
            currentHolderId: null, lastBorrowedDate: null);

    private static InventoryItem MakeLoanedItem(MemberId borrower)
    {
        var item = MakeAvailableItem();
        Assert.True(item.Loan(borrower).IsSuccess);
        item.ClearDomainEvents();
        return item;
    }

    private static Reservation MakePendingReservation() =>
        Reservation.Create(
            new ReservationId(1),
            new InventoryItemId(1),
            new MemberId(2),
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(15),
            ReservationStatus.Pending,
            isConfirmed: false,
            queuePosition: 1);
}
