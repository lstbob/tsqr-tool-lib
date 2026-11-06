using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;
using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Services;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class ReservationQueueServiceTests
{
    private static readonly InventoryItemId ItemId = new(1);
    private static readonly DateTime ReservationDate = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ShiftQueueAfterCancellation_WhenQueueHasOneReservation_LeavesItUnchanged()
    {
        var only = MakeReservation(1, 1);

        var result = new ReservationQueueService()
            .ShiftQueueAfterCancellation([only], only);

        Assert.Equal(1, only.QueuePosition);
        Assert.Same(only, Assert.Single(result));
    }

    [Fact]
    public void ShiftQueueAfterCancellation_WhenCancellingFirst_ShiftsFollowingReservationsUp()
    {
        var first = MakeReservation(1, 1);
        var second = MakeReservation(2, 2);
        var third = MakeReservation(3, 3);
        var queue = new[] { first, second, third };

        new ReservationQueueService().ShiftQueueAfterCancellation(queue, first);

        Assert.Equal([1, 1, 2], queue.Select(r => r.QueuePosition));
    }

    [Fact]
    public void ShiftQueueAfterCancellation_WhenCancellingMiddle_TightensTheGap()
    {
        var first = MakeReservation(1, 1);
        var cancelled = MakeReservation(2, 2);
        var third = MakeReservation(3, 3);
        var fourth = MakeReservation(4, 4);
        var queue = new[] { first, cancelled, third, fourth };

        new ReservationQueueService().ShiftQueueAfterCancellation(queue, cancelled);

        Assert.Equal(1, first.QueuePosition);
        Assert.Equal(2, cancelled.QueuePosition);
        Assert.Equal(2, third.QueuePosition);
        Assert.Equal(3, fourth.QueuePosition);
    }

    [Fact]
    public void ShiftQueueAfterCancellation_WhenCancellingLast_LeavesQueueUnchanged()
    {
        var first = MakeReservation(1, 1);
        var second = MakeReservation(2, 2);
        var last = MakeReservation(3, 3);
        var queue = new[] { first, second, last };

        new ReservationQueueService().ShiftQueueAfterCancellation(queue, last);

        Assert.Equal([1, 2], queue.Take(2).Select(r => r.QueuePosition));
        Assert.Equal(3, last.QueuePosition);
    }

    [Fact]
    public void CalculateNextQueuePosition_ReturnsOneBeyondCurrentQueueSize()
    {
        var service = new ReservationQueueService();

        Assert.Equal(1, service.CalculateNextQueuePosition([]));
        Assert.Equal(
            4,
            service.CalculateNextQueuePosition(
                [MakeReservation(1, 1), MakeReservation(2, 2), MakeReservation(3, 3)]));
    }

    private static Reservation MakeReservation(int id, int queuePosition) =>
        Reservation.Create(
            new ReservationId(id),
            ItemId,
            new MemberId(id + 100),
            ReservationDate,
            ReservationDate.AddDays(14),
            ReservationStatus.Pending,
            isConfirmed: false,
            queuePosition: queuePosition);
}
