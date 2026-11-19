using Microsoft.Extensions.Logging.Abstractions;
using TSQR.Common.Errors;
using TSQR.Common.Events;
using TSQR.ToolLibrary.Application;
using TSQR.ToolLibrary.Application.Errors;
using TSQR.ToolLibrary.Application.Loan.Commands;
using TSQR.ToolLibrary.Application.Member.Accounts;
using TSQR.ToolLibrary.Application.Member.Commands;
using TSQR.ToolLibrary.Application.Reservation.Commands;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.LocationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.PolicyAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class OwnershipAuthorizationTests
{
    private static readonly MemberId OwnerId = new(7);
    private static readonly MemberId OtherMemberId = new(8);

    [Fact]
    public async Task CancelReservation_OtherMember_IsForbiddenAndNotSaved()
    {
        var reservation = MakeReservation(OwnerId);
        var repository = new FakeReservationRepository(reservation);
        var uow = new NoopUnitOfWork();
        var handler = new CancelReservationCommandHandler(repository, MakeOrchestrator(uow));

        var result = await handler.ExecuteAsync(
            new CancelReservationCommand(
                reservation.Id,
                ActorMemberId: OtherMemberId,
                CanManageAny: false),
            CancellationToken.None);

        var error = Assert.IsType<ForbiddenError>(result.Error);
        Assert.Equal("Forbidden", error.Code);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal(0, uow.SaveChangesAsyncCallCount);
        Assert.False(repository.UpdateCalled);
    }

    [Fact]
    public async Task CancelReservation_Owner_IsAllowed()
    {
        var reservation = MakeReservation(OwnerId);
        var repository = new FakeReservationRepository(reservation);
        var uow = new NoopUnitOfWork();
        var handler = new CancelReservationCommandHandler(repository, MakeOrchestrator(uow));

        var result = await handler.ExecuteAsync(
            new CancelReservationCommand(reservation.Id, ActorMemberId: OwnerId),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.True(repository.UpdateCalled);
        Assert.Equal(1, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task CancelReservation_StaffCapability_CanManageAnotherMembersReservation()
    {
        var reservation = MakeReservation(OwnerId);
        var repository = new FakeReservationRepository(reservation);
        var uow = new NoopUnitOfWork();
        var handler = new CancelReservationCommandHandler(repository, MakeOrchestrator(uow));

        var result = await handler.ExecuteAsync(
            new CancelReservationCommand(
                reservation.Id,
                ActorMemberId: OtherMemberId,
                CanManageAny: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public async Task ReserveTool_ForAnotherMember_IsForbiddenBeforeRepositoriesAreUsed()
    {
        var handler = new ReserveToolCommandHandler(
            new FakeReservationRepository(null),
            new NoopInventoryRepository(),
            new NoopToolRepository(),
            new NoopPolicyRepository(),
            MakeOrchestrator(new NoopUnitOfWork()));

        var result = await handler.ExecuteAsync(
            new ReserveToolCommand(
                new InventoryItemId(1),
                OwnerId,
                DateTime.UtcNow,
                ActorMemberId: OtherMemberId),
            CancellationToken.None);

        Assert.IsType<ForbiddenError>(result.Error);
    }

    [Fact]
    public async Task LoanTool_ForAnotherMember_IsForbiddenBeforeMemberIsLoaded()
    {
        var handler = new LoanToolCommandHandler(
            new NoopMemberRepository(),
            new NoopLoanRepository(),
            new NoopInventoryRepository(),
            new NoopToolRepository(),
            new NoopPolicyRepository(),
            new FakeReservationRepository(null),
            MakeOrchestrator(new NoopUnitOfWork()));

        var result = await handler.ExecuteAsync(
            new LoanToolCommand(
                new InventoryItemId(1),
                OwnerId,
                ActorMemberId: OtherMemberId),
            CancellationToken.None);

        Assert.IsType<ForbiddenError>(result.Error);
    }

    [Fact]
    public async Task LinkMemberAccount_NewSubject_LinksAndSaves()
    {
        var member = MakeMember(OwnerId);
        var accounts = new FakeMemberAccountRepository(null);
        var uow = new NoopUnitOfWork();
        var handler = new LinkMemberAccountCommandHandler(
            new FakeMemberRepository(member),
            accounts,
            uow);

        var result = await handler.ExecuteAsync(
            new LinkMemberAccountCommand(OwnerId, Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(accounts.LinkCalled);
        Assert.Equal(1, uow.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task LinkMemberAccount_SubjectAlreadyLinkedElsewhere_IsRejected()
    {
        var member = MakeMember(OwnerId);
        var accounts = new FakeMemberAccountRepository(OtherMemberId);
        var handler = new LinkMemberAccountCommandHandler(
            new FakeMemberRepository(member),
            accounts,
            new NoopUnitOfWork());

        var result = await handler.ExecuteAsync(
            new LinkMemberAccountCommand(OwnerId, Guid.NewGuid()),
            CancellationToken.None);

        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal("AuthSubject", error.Code);
        Assert.False(accounts.LinkCalled);
    }

    private static Reservation MakeReservation(MemberId memberId) =>
        Reservation.Create(
            new ReservationId(1),
            new InventoryItemId(1),
            memberId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7),
            ReservationStatus.Pending,
            isConfirmed: false,
            queuePosition: 1,
            communityId: 1);

    private static Member MakeMember(MemberId id) =>
        Member.Create(
            id,
            "Alice",
            "",
            "Smith",
            30,
            "1 Main St",
            "alice@example.com",
            "555-0100",
            MemberStatus.Active,
            isVerified: true,
            verifiedByAdminId: new MemberId(99),
            verificationDate: DateTime.UtcNow,
            communityId: 1);

    private static DomainEventOrchestrator MakeOrchestrator(NoopUnitOfWork uow) =>
        new(
            uow,
            new NoopDispatcher(),
            new DomainEventCollector(),
            NullLogger<DomainEventOrchestrator>.Instance);

    private sealed class FakeReservationRepository(Reservation? reservation)
        : IReservationRepository
    {
        public bool UpdateCalled { get; private set; }

        public Task<Reservation?> GetByIdAsync(
            ReservationId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(reservation is not null && id == reservation.Id ? reservation : null);

        public Task AddAsync(Reservation entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Reservation entity) => UpdateCalled = true;

        public void Delete(Reservation entity) { }

        public Task<IReadOnlyCollection<Reservation>> GetByItemIdAsync(
            InventoryItemId itemId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Reservation>>(Array.Empty<Reservation>());
    }

    private sealed class FakeMemberRepository(Member? member) : IRepository<Member, MemberId>
    {
        public Task<Member?> GetByIdAsync(
            MemberId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(member is not null && id == member.Id ? member : null);

        public Task AddAsync(Member entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Member entity) { }

        public void Delete(Member entity) { }
    }

    private sealed class FakeMemberAccountRepository(MemberId? existing)
        : IMemberAccountRepository
    {
        public bool LinkCalled { get; private set; }

        public Task<MemberId?> GetBySubjectAsync(
            Guid authSubject,
            CancellationToken ct = default) =>
            Task.FromResult(existing);

        public Task LinkAsync(
            MemberId memberId,
            Guid authSubject,
            CancellationToken ct = default)
        {
            LinkCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class NoopInventoryRepository : IRepository<InventoryItem, InventoryItemId>
    {
        public Task<InventoryItem?> GetByIdAsync(
            InventoryItemId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<InventoryItem?>(null);

        public Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(InventoryItem entity) { }

        public void Delete(InventoryItem entity) { }
    }

    private sealed class NoopMemberRepository : IRepository<Member, MemberId>
    {
        public Task<Member?> GetByIdAsync(
            MemberId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Member?>(null);

        public Task AddAsync(Member entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Member entity) { }

        public void Delete(Member entity) { }
    }

    private sealed class NoopToolRepository : IToolRepository
    {
        public Task<Tool?> GetByIdAsync(
            ToolId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Tool?>(null);

        public Task AddAsync(Tool entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Tool entity) { }

        public void Delete(Tool entity) { }
    }

    private sealed class NoopPolicyRepository : IPolicyRepository
    {
        public Task<Policy?> GetByIdAsync(
            PolicyId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Policy?>(null);

        public Task<Policy?> GetByToolTypeAsync(
            ToolType toolType,
            LocationId? locationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Policy?>(null);

        public Task AddAsync(Policy entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Policy entity) { }

        public void Delete(Policy entity) { }
    }

    private sealed class NoopLoanRepository : ILoanRepository
    {
        public Task<Loan?> GetByIdAsync(
            LoanId id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Loan?>(null);

        public Task AddAsync(Loan entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Loan entity) { }

        public void Delete(Loan entity) { }

        public Task<IReadOnlyCollection<Loan>> GetActiveLoansByMemberAsync(
            MemberId memberId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Loan>>(Array.Empty<Loan>());

        public Task<decimal> GetOutstandingFineBalanceAsync(
            MemberId memberId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(0m);
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
