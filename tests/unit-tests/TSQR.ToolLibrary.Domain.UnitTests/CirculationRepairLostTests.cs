using TSQR.Common.Errors;
using TSQR.ToolLibrary.Application.Events;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class CirculationRepairLostTests
{
    private static readonly MemberId HolderId = new(7);
    private static readonly MemberId ReporterId = new(8);
    private static readonly InventoryItemId ItemId = new(1);
    private static readonly ToolId ToolId = new(1);

    [Fact]
    public void MarkForRepair_OnLoanedItem_IsRejectedAndKeepsState()
    {
        var item = MakeItem(ItemStatus.Loaned, HolderId);

        var result = item.MarkForRepair(ReporterId, "Broken handle");

        Assert.True(result.IsFailure);
        Assert.IsType<DomainError>(result.Error);
        Assert.Equal(ItemStatus.Loaned, item.Status);
        Assert.Equal(HolderId, item.CurrentHolderId);
        Assert.Empty(item.DomainEvents);
    }

    [Fact]
    public void MarkForRepair_OnLostItem_IsRejected()
    {
        var item = MakeItem(ItemStatus.Lost);

        var result = item.MarkForRepair(ReporterId, "Broken handle");

        Assert.True(result.IsFailure);
        Assert.IsType<DomainError>(result.Error);
        Assert.Equal(ItemStatus.Lost, item.Status);
    }

    [Fact]
    public void MarkAsLost_WithoutReporter_ReturnsValidationError()
    {
        var item = MakeItem(ItemStatus.Loaned, HolderId);

        var result = item.MarkAsLost(null!);

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(ItemStatus.Loaned, item.Status);
    }

    [Fact]
    public void MarkAsLost_OnLoanedItem_ClearsHolderAndRaisesItemLost()
    {
        var item = MakeItem(ItemStatus.Loaned, HolderId);

        var result = item.MarkAsLost(ReporterId);

        Assert.True(result.IsSuccess);
        Assert.Equal(ItemStatus.Lost, item.Status);
        Assert.Null(item.CurrentHolderId);
        var domainEvent = Assert.IsType<ItemLostDomainEvent>(Assert.Single(item.DomainEvents));
        Assert.Equal(ReporterId, domainEvent.ReportedByMemberId);
        Assert.Equal(HolderId, domainEvent.PreviousHolderId);
    }

    [Fact]
    public void MarkAsLost_WithOpenRepair_CancelsTheRepair()
    {
        var item = MakeItem(ItemStatus.Available);
        Assert.True(item.MarkForRepair(ReporterId, "Broken handle").IsSuccess);
        Assert.True(item.IsUnderRepair);

        var result = item.MarkAsLost(ReporterId);

        Assert.True(result.IsSuccess);
        Assert.False(item.IsUnderRepair);
        Assert.True(item.CompleteRepair(ReporterId, Condition.Good).IsFailure);
    }

    [Fact]
    public void Loan_MarkItemLost_ClosesActiveLoan()
    {
        var loan = MakeActiveLoan(DateTime.UtcNow.AddDays(3));

        var result = loan.MarkItemLost(DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(LoanStatus.Lost, loan.Status);
        Assert.Equal(0m, loan.FineAccrued);
    }

    [Fact]
    public void Loan_MarkItemLost_AfterDueDate_AccruesFine()
    {
        var loan = MakeActiveLoan(DateTime.UtcNow.AddDays(-3));

        var result = loan.MarkItemLost(DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(LoanStatus.Lost, loan.Status);
        Assert.True(loan.FineAccrued > 0m);
    }

    [Fact]
    public void Loan_MarkItemLost_OnReturnedLoan_IsRejected()
    {
        var loan = MakeActiveLoan(DateTime.UtcNow.AddDays(3), LoanStatus.Returned);

        var result = loan.MarkItemLost(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.IsType<DomainError>(result.Error);
        Assert.Equal(LoanStatus.Returned, loan.Status);
    }

    [Fact]
    public async Task ItemLostEventHandler_ClosesLoanAndCancelsOpenReservations()
    {
        var loan = MakeActiveLoan(DateTime.UtcNow.AddDays(3));
        var loanRepository = new FakeLoanRepository(loan);
        var pending = MakeReservation(1, ReservationStatus.Pending);
        var confirmed = MakeReservation(2, ReservationStatus.Confirmed);
        var active = MakeReservation(3, ReservationStatus.Active);
        var completed = MakeReservation(4, ReservationStatus.Completed);
        var reservationRepository = new FakeReservationRepository([pending, confirmed, active, completed]);
        var handler = new ItemLostEventHandler(loanRepository, reservationRepository);

        await handler.HandleAsync(
            new ItemLostDomainEvent(ItemId, ReporterId, HolderId),
            CancellationToken.None);

        Assert.Equal(LoanStatus.Lost, loan.Status);
        Assert.Equal(1, loanRepository.UpdateCallCount);
        Assert.Equal(ReservationStatus.Cancelled, pending.Status);
        Assert.Equal(ReservationStatus.Cancelled, confirmed.Status);
        Assert.Equal(ReservationStatus.Cancelled, active.Status);
        Assert.Equal(ReservationStatus.Completed, completed.Status);
        Assert.Equal(3, reservationRepository.Updated.Count);
        Assert.DoesNotContain(completed, reservationRepository.Updated);
    }

    [Fact]
    public async Task ReservationCancelledEventHandler_LastReservation_ReleasesReservedItem()
    {
        var item = MakeItem(ItemStatus.Reserved);
        var inventoryRepository = new FakeInventoryRepository(item);
        var cancelled = MakeReservation(1, ReservationStatus.Cancelled);
        var handler = new ReservationCancelledEventHandler(
            new FakeReservationRepository([cancelled]),
            inventoryRepository);

        await handler.HandleAsync(
            new ReservationCancelledEvent(cancelled.Id, ItemId, HolderId),
            CancellationToken.None);

        Assert.Equal(ItemStatus.Available, item.Status);
        Assert.Equal(1, inventoryRepository.UpdateCallCount);
    }

    [Fact]
    public async Task ReservationCancelledEventHandler_LastReservation_LeavesItemUnderMaintenance()
    {
        var item = MakeItem(ItemStatus.UnderMaintenance);
        var inventoryRepository = new FakeInventoryRepository(item);
        var cancelled = MakeReservation(1, ReservationStatus.Cancelled);
        var handler = new ReservationCancelledEventHandler(
            new FakeReservationRepository([cancelled]),
            inventoryRepository);

        await handler.HandleAsync(
            new ReservationCancelledEvent(cancelled.Id, ItemId, HolderId),
            CancellationToken.None);

        Assert.Equal(ItemStatus.UnderMaintenance, item.Status);
        Assert.Equal(0, inventoryRepository.UpdateCallCount);
    }

    private static InventoryItem MakeItem(ItemStatus status, MemberId? holderId = null) =>
        InventoryItem.Create(
            ItemId,
            ToolId,
            HolderId,
            DateTime.UtcNow.AddDays(-10),
            "SN-001",
            status,
            Condition.Good,
            currentHolderId: holderId,
            lastBorrowedDate: null,
            communityId: 1);

    private static Loan MakeActiveLoan(DateTime dueDate, LoanStatus status = LoanStatus.Active) =>
        Loan.Rehydrate(
            new LoanId(10),
            HolderId,
            checkoutDate: dueDate.AddDays(-7),
            dueDate,
            ItemId,
            status,
            returnedDate: null,
            fineAccrued: 0m,
            lateFeePerDay: 1m,
            renewalCount: 0,
            communityId: 1);

    private static Reservation MakeReservation(int id, ReservationStatus status) =>
        Reservation.Create(
            new ReservationId(id),
            ItemId,
            new MemberId(20 + id),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(14),
            status,
            isConfirmed: false,
            queuePosition: id,
            communityId: 1);

    private sealed class FakeLoanRepository(Loan? activeLoan) : ILoanRepository
    {
        public int UpdateCallCount { get; private set; }

        public Task<Loan?> GetByIdAsync(
            LoanId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Loan?>(null);

        public Task AddAsync(Loan entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Loan entity) => UpdateCallCount++;

        public void Delete(Loan entity) { }

        public Task<IReadOnlyCollection<Loan>> GetActiveLoansByMemberAsync(
            MemberId memberId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Loan>>(Array.Empty<Loan>());

        public Task<Loan?> GetActiveLoanByItemAsync(
            InventoryItemId itemId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(activeLoan);

        public Task<decimal> GetOutstandingFineBalanceAsync(
            MemberId memberId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0m);
    }

    private sealed class FakeReservationRepository(IReadOnlyCollection<Reservation> reservations)
        : IReservationRepository
    {
        public List<Reservation> Updated { get; } = [];

        public Task<Reservation?> GetByIdAsync(
            ReservationId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Reservation?>(null);

        public Task AddAsync(Reservation entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Reservation entity) => Updated.Add(entity);

        public void Delete(Reservation entity) { }

        public Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(
            InventoryItemId itemId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(reservations);
    }

    private sealed class FakeInventoryRepository(InventoryItem item)
        : IRepository<InventoryItem, InventoryItemId>
    {
        public int UpdateCallCount { get; private set; }

        public Task<InventoryItem?> GetByIdAsync(
            InventoryItemId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<InventoryItem?>(id == item.Id ? item : null);

        public Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(InventoryItem entity) => UpdateCallCount++;

        public void Delete(InventoryItem entity) { }
    }
}
