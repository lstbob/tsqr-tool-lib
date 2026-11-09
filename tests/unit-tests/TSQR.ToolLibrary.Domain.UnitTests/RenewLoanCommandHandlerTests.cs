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
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class RenewLoanCommandHandlerTests
{
    private static readonly MemberId BorrowerId = new(7);
    private static readonly InventoryItemId ItemId = new(1);
    private static readonly ToolId ToolId = new(1);

    [Fact]
    public async Task Renew_ActiveLoan_ExtendsDueDateAndPersists()
    {
        var loan = MakeLoan();
        var originalDueDate = loan.DueDate;
        var loanRepository = new FakeLoanRepository(loan);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, MakeItem(), MakeTool(), MakePolicy(), uow);

        var result = await handler.ExecuteAsync(new RenewLoanCommand(loan.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, loan.RenewalCount);
        Assert.Equal(originalDueDate.AddDays(7), loan.DueDate);
        Assert.Equal(1, loanRepository.UpdateCount);
        Assert.Equal(1, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task Renew_MissingLoan_ReturnsNotFoundWithoutSaving()
    {
        var loanRepository = new FakeLoanRepository(null);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, null, null, null, uow);

        var result = await handler.ExecuteAsync(new RenewLoanCommand(new LoanId(99)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.IsType<NotFoundError>(result.Error);
        Assert.Equal(0, loanRepository.UpdateCount);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task Renew_MissingInventoryItem_ReturnsNotFoundWithoutSaving()
    {
        var loan = MakeLoan();
        var loanRepository = new FakeLoanRepository(loan);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, null, MakeTool(), MakePolicy(), uow);

        var result = await handler.ExecuteAsync(new RenewLoanCommand(loan.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.IsType<NotFoundError>(result.Error);
        Assert.Equal(0, loanRepository.UpdateCount);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task Renew_MissingTool_ReturnsNotFoundWithoutSaving()
    {
        var loan = MakeLoan();
        var loanRepository = new FakeLoanRepository(loan);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, MakeItem(), null, MakePolicy(), uow);

        var result = await handler.ExecuteAsync(new RenewLoanCommand(loan.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.IsType<NotFoundError>(result.Error);
        Assert.Equal(0, loanRepository.UpdateCount);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task Renew_MissingPolicy_ReturnsDomainErrorWithoutSaving()
    {
        var loan = MakeLoan();
        var loanRepository = new FakeLoanRepository(loan);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, MakeItem(), MakeTool(), null, uow);

        var result = await handler.ExecuteAsync(new RenewLoanCommand(loan.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.IsType<DomainError>(result.Error);
        Assert.Equal(0, loanRepository.UpdateCount);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task Renew_AtPolicyRenewalLimit_ReturnsDomainErrorWithoutSaving()
    {
        var loan = MakeLoan(renewalCount: 2);
        var loanRepository = new FakeLoanRepository(loan);
        var uow = new NoopUnitOfWork();
        var handler = CreateHandler(loanRepository, MakeItem(), MakeTool(), MakePolicy(), uow);

        var result = await handler.ExecuteAsync(new RenewLoanCommand(loan.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.IsType<DomainError>(result.Error);
        Assert.Equal(2, loan.RenewalCount);
        Assert.Equal(0, loanRepository.UpdateCount);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
    }

    private static RenewLoanCommandHandler CreateHandler(
        FakeLoanRepository loanRepository,
        InventoryItem? item,
        Tool? tool,
        Policy? policy,
        NoopUnitOfWork uow)
    {
        var orchestrator = new DomainEventOrchestrator(
            uow,
            new NoopDispatcher(),
            new DomainEventCollector(),
            NullLogger<DomainEventOrchestrator>.Instance);

        return new RenewLoanCommandHandler(
            loanRepository,
            new FakeInventoryRepository(item),
            new FakeToolRepository(tool),
            new FakePolicyRepository(policy),
            orchestrator);
    }

    private static Loan MakeLoan(int renewalCount = 0)
    {
        var now = DateTime.UtcNow;
        var result = Loan.Create(
            new LoanId(1),
            BorrowerId,
            now.AddDays(-2),
            now.AddDays(5),
            ItemId,
            LoanStatus.Active,
            lateFeePerDay: 1m,
            renewalCount: renewalCount,
            communityId: 1);
        Assert.True(result.IsSuccess);
        var loan = result.Value;
        loan.ClearDomainEvents();
        return loan;
    }

    private static InventoryItem MakeItem() =>
        InventoryItem.Create(
            ItemId,
            ToolId,
            BorrowerId,
            DateTime.UtcNow.AddDays(-10),
            "SN-001",
            ItemStatus.Loaned,
            Condition.Good,
            currentHolderId: BorrowerId,
            lastBorrowedDate: DateTime.UtcNow.AddDays(-2));

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

    private sealed class FakeLoanRepository(Loan? loan) : IRepository<Loan, LoanId>
    {
        public int UpdateCount { get; private set; }

        public Task<Loan?> GetByIdAsync(
            LoanId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(loan is not null && id == loan.Id ? loan : null);

        public Task AddAsync(Loan entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Loan entity) => UpdateCount++;

        public void Delete(Loan entity) { }
    }

    private sealed class FakeInventoryRepository(InventoryItem? item)
        : IRepository<InventoryItem, InventoryItemId>
    {
        public Task<InventoryItem?> GetByIdAsync(
            InventoryItemId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(item is not null && id == item.Id ? item : null);

        public Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(InventoryItem entity) { }

        public void Delete(InventoryItem entity) { }
    }

    private sealed class FakeToolRepository(Tool? tool) : IToolRepository
    {
        public Task<Tool?> GetByIdAsync(
            ToolId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(tool is not null && id == tool.Id ? tool : null);

        public Task AddAsync(Tool entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Tool entity) { }

        public void Delete(Tool entity) { }
    }

    private sealed class FakePolicyRepository(Policy? policy) : IPolicyRepository
    {
        public Task<Policy?> GetByIdAsync(
            PolicyId id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Policy?>(null);

        public Task<Policy?> GetByToolTypeAsync(
            ToolType toolType, LocationId? locationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(policy);

        public Task AddAsync(Policy entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Policy entity) { }

        public void Delete(Policy entity) { }
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
