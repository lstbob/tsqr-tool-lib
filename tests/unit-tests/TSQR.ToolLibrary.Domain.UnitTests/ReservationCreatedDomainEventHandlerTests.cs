using TSQR.ToolLibrary.Application.Events;
using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class ReservationCreatedDomainEventHandlerTests
{
    private static readonly InventoryItemId ItemId = new(1);
    private static readonly DateTime Now = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_FirstReservationOnAvailableItem_ReservesItem()
    {
        var item = MakeItem(ItemStatus.Available);
        var reservation = MakeReservation(1, 1);
        var inventory = new FakeInventoryRepository(item);
        var handler = new ReservationCreatedDomainEventHandler(inventory);

        await handler.HandleAsync(ToEvent(reservation), CancellationToken.None);

        Assert.Equal(ItemStatus.Reserved, item.Status);
        Assert.Equal(1, inventory.UpdateCount);
    }

    [Fact]
    public async Task HandleAsync_FirstReservationOnLoanedItem_JoinsQueueWithoutThrowing()
    {
        var item = MakeItem(ItemStatus.Loaned);
        var reservation = MakeReservation(1, 1);
        var inventory = new FakeInventoryRepository(item);
        var handler = new ReservationCreatedDomainEventHandler(inventory);

        await handler.HandleAsync(ToEvent(reservation), CancellationToken.None);

        Assert.Equal(ItemStatus.Loaned, item.Status);
        Assert.Equal(0, inventory.UpdateCount);
    }

    [Fact]
    public async Task HandleAsync_SubsequentReservation_DoesNotHoldItemAgain()
    {
        var item = MakeItem(ItemStatus.Available);
        var reservation = MakeReservation(2, 2);
        var inventory = new FakeInventoryRepository(item);
        var handler = new ReservationCreatedDomainEventHandler(inventory);

        await handler.HandleAsync(ToEvent(reservation), CancellationToken.None);

        Assert.Equal(ItemStatus.Available, item.Status);
        Assert.Equal(0, inventory.UpdateCount);
    }

    [Fact]
    public async Task HandleAsync_EventCarriesPreAssignmentId_StillReservesFirstReservation()
    {
        var item = MakeItem(ItemStatus.Available);
        var reservation = MakeReservation(7, 1);
        var inventory = new FakeInventoryRepository(item);
        var handler = new ReservationCreatedDomainEventHandler(inventory);
        var preAssignmentEvent = new ReservationCreatedDomainEvent(
            new ReservationId(0),
            reservation.ItemId,
            reservation.MemberId,
            reservation.ReservationDate,
            reservation.QueuePosition);

        await handler.HandleAsync(preAssignmentEvent, CancellationToken.None);

        Assert.Equal(ItemStatus.Reserved, item.Status);
        Assert.Equal(1, inventory.UpdateCount);
    }

    [Fact]
    public async Task HandleAsync_FirstReservationOnReservedItem_JoinsQueueWithoutThrowing()
    {
        var item = MakeItem(ItemStatus.Reserved);
        var reservation = MakeReservation(1, 1);
        var inventory = new FakeInventoryRepository(item);
        var handler = new ReservationCreatedDomainEventHandler(inventory);

        await handler.HandleAsync(ToEvent(reservation), CancellationToken.None);

        Assert.Equal(ItemStatus.Reserved, item.Status);
        Assert.Equal(0, inventory.UpdateCount);
    }

    private static ReservationCreatedDomainEvent ToEvent(Reservation reservation) =>
        new(
            reservation.Id,
            reservation.ItemId,
            reservation.MemberId,
            reservation.ReservationDate,
            reservation.QueuePosition);

    private static InventoryItem MakeItem(ItemStatus status) =>
        InventoryItem.Create(
            ItemId,
            new ToolId(1),
            new MemberId(1),
            Now.AddDays(-10),
            "SN-001",
            status,
            Condition.New,
            currentHolderId: status == ItemStatus.Loaned ? new MemberId(2) : null,
            lastBorrowedDate: status == ItemStatus.Loaned ? Now.AddDays(-1) : null);

    private static Reservation MakeReservation(
        int id,
        int queuePosition,
        ReservationStatus status = ReservationStatus.Pending) =>
        Reservation.Create(
            new ReservationId(id),
            ItemId,
            new MemberId(id + 100),
            Now.AddDays(1),
            Now.AddDays(15),
            status,
            isConfirmed: false,
            queuePosition: queuePosition);

    private sealed class FakeInventoryRepository(InventoryItem item)
        : IRepository<InventoryItem, InventoryItemId>
    {
        public int UpdateCount { get; private set; }

        public IUnitOfWork UnitOfWork { get; } = new NoopUnitOfWork();

        public Task<InventoryItem?> GetByIdAsync(
            InventoryItemId id, CancellationToken cancellationToken = default) =>
            Task.FromResult<InventoryItem?>(id == item.Id ? item : null);

        public Task AddAsync(InventoryItem entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(InventoryItem entity) => UpdateCount++;

        public void Delete(InventoryItem entity) { }
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }
}
