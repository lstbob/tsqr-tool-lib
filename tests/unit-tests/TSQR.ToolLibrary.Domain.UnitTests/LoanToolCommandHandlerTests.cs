using Microsoft.Extensions.Logging.Abstractions;
using TSQR.Common.Errors;
using TSQR.Common.Events;
using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Application.Loan.Commands;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.PolicyAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class LoanToolCommandHandlerTests
{
    private static readonly MemberId BorrowerId = new(7);
    private static readonly InventoryItemId ItemId = new(1);
    private static readonly ToolId ToolId = new(1);

    [Fact]
    public async Task LoanTool_ValidRequest_CreatesLoanAndSaves()
    {
        var item = MakeItem();
        var loanRepository = new FakeLoanRepository();
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, uow, item, MakeTool(), MakePolicy());

        var result = await handler.ExecuteAsync(
            new LoanToolCommand(ItemId, BorrowerId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var loan = Assert.Single(loanRepository.AddedLoans);
        Assert.Equal(BorrowerId, loan.MemberId);
        Assert.Equal(ItemId, loan.ItemId);
        Assert.Equal(1, uow.SaveChangesAsyncCallCount);
    }

    [Theory]
    [InlineData(ItemStatus.Loaned)]
    [InlineData(ItemStatus.Lost)]
    [InlineData(ItemStatus.UnderMaintenance)]
    [InlineData(ItemStatus.NotSet)]
    public async Task LoanTool_NonLoanableItem_ReturnsDomainError(ItemStatus status)
    {
        var item = MakeItem(status);
        var loanRepository = new FakeLoanRepository();
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, uow, item, MakeTool(), MakePolicy());

        var result = await handler.ExecuteAsync(
            new LoanToolCommand(ItemId, BorrowerId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("Status", error.Code);
        Assert.Empty(loanRepository.AddedLoans);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task LoanTool_MemberHasOverdueLoan_ReturnsDomainError()
    {
        var overdueLoan = MakeActiveLoan(
            loanId: new LoanId(10),
            itemId: new InventoryItemId(2),
            dueDate: DateTime.UtcNow.AddDays(-1));
        var loanRepository = new FakeLoanRepository([overdueLoan]);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, uow, MakeItem(), MakeTool(), MakePolicy());

        var result = await handler.ExecuteAsync(
            new LoanToolCommand(ItemId, BorrowerId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("activeOverdueLoanCount", error.Code);
        Assert.Empty(loanRepository.AddedLoans);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task LoanTool_MemberExceedsFineLimit_ReturnsDomainError()
    {
        var loanRepository = new FakeLoanRepository(
            outstandingFineBalance: Member.MaxOutstandingFineBalance + 0.01m);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, uow, MakeItem(), MakeTool(), MakePolicy());

        var result = await handler.ExecuteAsync(
            new LoanToolCommand(ItemId, BorrowerId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("outstandingFineBalance", error.Code);
        Assert.Empty(loanRepository.AddedLoans);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task LoanTool_MemberAlreadyHasActiveLoanForSameItem_ReturnsDomainError()
    {
        var activeLoan = MakeActiveLoan(
            loanId: new LoanId(10),
            itemId: ItemId,
            dueDate: DateTime.UtcNow.AddDays(1));
        var loanRepository = new FakeLoanRepository([activeLoan]);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, uow, MakeItem(), MakeTool(), MakePolicy());

        var result = await handler.ExecuteAsync(
            new LoanToolCommand(ItemId, BorrowerId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("ItemId", error.Code);
        Assert.Empty(loanRepository.AddedLoans);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task LoanTool_SuspendedMember_ReturnsDomainError()
    {
        var loanRepository = new FakeLoanRepository();
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(
            loanRepository,
            uow,
            MakeItem(),
            MakeTool(),
            MakePolicy(),
            MakeMember(MemberStatus.Suspended));

        var result = await handler.ExecuteAsync(
            new LoanToolCommand(ItemId, BorrowerId),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("Status", error.Code);
        Assert.Empty(loanRepository.AddedLoans);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    private static LoanToolCommandHandler CreateHandler(
        FakeLoanRepository loanRepository,
        NoopUnitOfWork uow,
        InventoryItem item,
        Tool tool,
        Policy policy,
        Member? member = null)
    {
        var orchestrator = new DomainEventOrchestrator(
            uow,
            new NoopDispatcher(),
            new DomainEventCollector(),
            NullLogger<DomainEventOrchestrator>.Instance);

        return new LoanToolCommandHandler(
            new FakeMemberRepository(member ?? MakeMember()),
            loanRepository,
            new FakeInventoryRepository(item),
            new FakeToolRepository(tool),
            new FakePolicyRepository(policy),
            new FakeReservationRepository(),
            orchestrator);
    }

    private static Member MakeMember(MemberStatus status = MemberStatus.Active) =>
        Member.Create(
            BorrowerId,
            "Alice",
            "",
            "Smith",
            30,
            "1 Main St",
            "alice@example.com",
            "555-0100",
            status,
            isVerified: true,
            verifiedByAdminId: new MemberId(99),
            verificationDate: DateTime.UtcNow,
            communityId: 1);

    private static Loan MakeActiveLoan(
        LoanId loanId,
        InventoryItemId itemId,
        DateTime dueDate) =>
        Loan.Rehydrate(
            loanId,
            BorrowerId,
            checkoutDate: dueDate.AddDays(-7),
            dueDate,
            itemId,
            LoanStatus.Active,
            returnedDate: null,
            fineAccrued: 0m,
            lateFeePerDay: 1m,
            renewalCount: 0,
            communityId: 1);

    private static InventoryItem MakeItem(ItemStatus status = ItemStatus.Available) =>
        InventoryItem.Create(
            ItemId,
            ToolId,
            BorrowerId,
            DateTime.UtcNow.AddDays(-10),
            "SN-001",
            status,
            Condition.Good,
            currentHolderId: null,
            lastBorrowedDate: null,
            communityId: 1);

    private static Tool MakeTool() =>
        Tool.Create(
            ToolId,
            "Cordless Drill",
            "18V cordless drill",
            Manufacturer.Create(new ManufacturerId(1), "Acme"),
            ToolType.PowerTool,
            AmortizationRate.Low);

    private static Policy MakePolicy()
    {
        var result = Policy.Create(
            new PolicyId(1),
            ToolType.PowerTool,
            locationId: null,
            "Power tools",
            "Default power-tool policy",
            lateFeePerDay: 1m,
            maxLoanDurationDays: 7,
            maxRenewalCount: 2,
            maxLoanReservationDays: 14);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private sealed class FakeMemberRepository(Member member)
        : IRepository<Member, MemberId>
    {
        public Task<Member?> GetByIdAsync(
            MemberId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Member?>(id == member.Id ? member : null);

        public Task AddAsync(Member entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Member entity) { }

        public void Delete(Member entity) { }
    }

    private sealed class FakeLoanRepository(
        IReadOnlyCollection<Loan>? activeLoans = null,
        decimal outstandingFineBalance = 0m)
        : ILoanRepository
    {
        public List<Loan> AddedLoans { get; } = [];

        public Task<Loan?> GetByIdAsync(
            LoanId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Loan?>(null);

        public Task AddAsync(Loan entity, CancellationToken cancellationToken = default)
        {
            AddedLoans.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Loan entity) { }

        public void Delete(Loan entity) { }

        public Task<IReadOnlyCollection<Loan>> GetActiveLoansByMemberAsync(
            MemberId memberId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(activeLoans ?? (IReadOnlyCollection<Loan>)Array.Empty<Loan>());

        public Task<decimal> GetOutstandingFineBalanceAsync(
            MemberId memberId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(outstandingFineBalance);
    }

    private sealed class FakeInventoryRepository(InventoryItem? item)
        : IRepository<InventoryItem, InventoryItemId>
    {
        public Task<InventoryItem?> GetByIdAsync(
            InventoryItemId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(item is not null && id == item.Id ? item : null);

        public Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(InventoryItem entity) { }

        public void Delete(InventoryItem entity) { }
    }

    private sealed class FakeToolRepository(Tool? tool) : IToolRepository
    {
        public Task<Tool?> GetByIdAsync(
            ToolId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(tool is not null && id == tool.Id ? tool : null);

        public Task AddAsync(Tool entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Tool entity) { }

        public void Delete(Tool entity) { }
    }

    private sealed class FakePolicyRepository(Policy? policy) : IPolicyRepository
    {
        public Task<Policy?> GetByIdAsync(
            PolicyId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Policy?>(null);

        public Task<Policy?> GetByToolTypeAsync(
            ToolType toolType,
            LocationId? locationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(policy);

        public Task AddAsync(Policy entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Policy entity) { }

        public void Delete(Policy entity) { }
    }

    private sealed class FakeReservationRepository : IReservationRepository
    {
        public Task<Reservation?> GetByIdAsync(
            ReservationId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Reservation?>(null);

        public Task AddAsync(Reservation entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Reservation entity) { }

        public void Delete(Reservation entity) { }

        public Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(
            InventoryItemId itemId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Reservation>>(Array.Empty<Reservation>());
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public int SaveChangesAsyncCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAsyncCallCount++;
            return Task.FromResult(0);
        }
    }

    private sealed class NoopDispatcher : IDomainEventDispatcher
    {
        public ValueTask DispatchAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask DispatchPostCommitAsync(
            IReadOnlyCollection<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }
}
