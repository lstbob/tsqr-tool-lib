using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Domain;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Domain.UnitTests;

/// <summary>
/// Phase 1 tests: prove the in-transaction rollback contract of
/// <see cref="DomainEventOrchestrator"/>. The orchestrator must dispatch
/// domain events to handlers BEFORE committing the database transaction,
/// so a handler (or commit) failure leaves the database untouched and the
/// aggregates' event collections intact for inspection. This is the eShop
/// "Option A" pattern - the user's stated #1 priority: domain invariants
/// and aggregate data must be valid at all times.
/// </summary>
public class DomainEventOrchestratorTests
{
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

    [Fact]
    public async Task SaveEntitiesAsync_WhenAllSucceed_DispatchesSavesAndClearsEvents()
    {
        var (aggregate, admin) = MakeMemberAndAdmin();
        Assert.True(aggregate.Verify(admin.Id).IsSuccess);
        var originalEvent = Assert.Single(aggregate.DomainEvents);

        var dispatched = new List<IDomainEvent>();
        var dispatcher = new RecordingDispatcher(dispatched);
        var uow = new RecordingUnitOfWork();
        var orchestrator = new DomainEventOrchestrator(uow, dispatcher);

        await orchestrator.SaveEntitiesAsync(aggregate, CancellationToken.None);

        // Dispatch happened BEFORE commit: the dispatcher recorded the
        // event reference, and SaveChangesAsync was called only once.
        Assert.Same(originalEvent, Assert.Single(dispatched));
        Assert.Equal(1, dispatcher.DispatchCallCount);
        Assert.Equal(1, uow.SaveChangesAsyncCallCount);
        // After success, events are cleared.
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public async Task SaveEntitiesAsync_WhenHandlerThrows_SaveChangesNotCalled_AndEventsRetained()
    {
        var (aggregate, admin) = MakeMemberAndAdmin();
        Assert.True(aggregate.Verify(admin.Id).IsSuccess);
        var originalEvent = Assert.Single(aggregate.DomainEvents);

        var dispatcher = new ThrowingDispatcher(new InvalidOperationException("handler boom"));
        var uow = new RecordingUnitOfWork();
        var orchestrator = new DomainEventOrchestrator(uow, dispatcher);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.SaveEntitiesAsync(aggregate, CancellationToken.None));

        Assert.Equal("handler boom", ex.Message);

        // The handler threw before commit: SaveChangesAsync was never called,
        // so the underlying transaction will roll back on dispose. The event
        // is still on the aggregate so a caller can inspect or retry.
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
        Assert.Same(originalEvent, Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public async Task SaveEntitiesAsync_WhenSaveChangesThrows_DoesNotClearEvents()
    {
        var (aggregate, admin) = MakeMemberAndAdmin();
        Assert.True(aggregate.Verify(admin.Id).IsSuccess);
        var originalEvent = Assert.Single(aggregate.DomainEvents);

        var dispatcher = new RecordingDispatcher(new List<IDomainEvent>());
        var uow = new ThrowingUnitOfWork(new InvalidOperationException("commit boom"));
        var orchestrator = new DomainEventOrchestrator(uow, dispatcher);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orchestrator.SaveEntitiesAsync(aggregate, CancellationToken.None));

        Assert.Equal("commit boom", ex.Message);

        // Dispatch happened (any side-effect SQL ran against the open
        // transaction), then commit threw - the transaction rolls back on
        // dispose, undoing every side effect. Events are NOT cleared, so
        // the caller can inspect what was attempted.
        Assert.Equal(1, dispatcher.DispatchCallCount);
        Assert.Same(originalEvent, Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public async Task SaveEntitiesAsync_MultipleAggregates_DispatchesAllEventsAndClearsAll()
    {
        // Mirror LoanToolCommand: loan + item both raise events; orchestrator
        // dispatches both sets in one transaction and clears both.
        var item = InventoryItem.Create(
            new InventoryItemId(1),
            new ToolId(1),
            new MemberId(1),
            DateTime.UtcNow.AddDays(-1),
            "SN-001",
            ItemStatus.Available,
            Condition.New,
            currentHolderId: null, lastBorrowedDate: null, reservationDate: null, reservationMemberId: null);
        Assert.True(item.Loan(new MemberId(2)).IsSuccess);
        Assert.Single(item.DomainEvents);

        var loanResult = Loan.Create(new MemberId(2), item.Id);
        Assert.True(loanResult.IsSuccess);
        var loan = loanResult.Value;

        var dispatched = new List<IDomainEvent>();
        var dispatcher = new RecordingDispatcher(dispatched);
        var uow = new RecordingUnitOfWork();
        var orchestrator = new DomainEventOrchestrator(uow, dispatcher);

        await orchestrator.SaveEntitiesAsync([loan, item], CancellationToken.None);

        Assert.Equal(1, dispatcher.DispatchCallCount);
        Assert.Single(dispatched);
        Assert.Empty(item.DomainEvents);
        Assert.Empty(loan.DomainEvents);
    }

    // ---- Fakes (technology-agnostic, prove the swappability contract) ----

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        public int SaveChangesAsyncCallCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAsyncCallCount++;
            return Task.FromResult(0);
        }
    }

    private sealed class ThrowingUnitOfWork : IUnitOfWork
    {
        private readonly Exception _ex;
        public ThrowingUnitOfWork(Exception ex) => _ex = ex;
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw _ex;
    }

    private sealed class RecordingDispatcher : IDomainEventDispatcher
    {
        private readonly List<IDomainEvent> _recorded;
        public int DispatchCallCount { get; private set; }
        public RecordingDispatcher(List<IDomainEvent> recorded) => _recorded = recorded;

        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            DispatchCallCount++;
            _recorded.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDispatcher : IDomainEventDispatcher
    {
        private readonly Exception _ex;
        public ThrowingDispatcher(Exception ex) => _ex = ex;
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
            => throw _ex;
    }
}