using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Events;
using TSQR.ToolLibrary.Domain.Services;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// Side effect of <see cref="Reservation.Cancel"/>:
///   1. Renumber <see cref="Reservation.QueuePosition"/> for every reservation
///      behind the cancelled slot via <see cref="ReservationQueueService.ShiftQueueAfterCancellation"/>
///      (closes gaps like 1, 3, 4 -> 1, 2, 3).
///   2. Find the next-in-line reservation (top of the pending/confirmed queue)
///      and notify it - which raises <see cref="NextInLineNotificationEvent"/>
///      on that reservation, prompting <see cref="NextInLineNotificationHandler"/>
///      to hold the InventoryItem for them.
/// All mutations are persisted in the originating unit-of-work transaction.
/// </summary>
public class ReservationCancelledEventHandler(
    IReservationRepository reservationRepository,
    IDomainEventDispatcher dispatcher)
    : IDomainEventHandler<ReservationCancelledEvent>
{
    public async Task HandleAsync(
        ReservationCancelledEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var allReservations = await reservationRepository.GetByItemIdAsync(
            domainEvent.ItemId, cancellationToken);

        var queueService = new ReservationQueueService();

        var cancelledReservation = allReservations
            .FirstOrDefault(r => r.Id == domainEvent.ReservationId);
        if (cancelledReservation is null)
            return;

        // 1. Tighten the queue behind the cancelled slot. Each shifted
        //    reservation is persisted so the new QueuePosition is durable.
        queueService.ShiftQueueAfterCancellation(allReservations, cancelledReservation);
        foreach (var reservation in allReservations.Where(r => !r.Id.Equals(cancelledReservation.Id)))
            reservationRepository.Update(reservation);

        // 2. Promote and notify the next-in-line member. NotifyNextInLine raises
        //    NextInLineNotificationEvent on the reservation; the dispatcher
        //    fans it out so NextInLineNotificationHandler runs in the same
        //    transaction to hold the InventoryItem for the promoted member.
        var nextInLine = queueService.GetNextInLine(allReservations);
        if (nextInLine is null)
            return;

        nextInLine.NotifyNextInLine(
            "A prior reservation was cancelled. The tool is now available for you.");
        reservationRepository.Update(nextInLine);

        // The single-pass dispatcher only iterates events present at the call
        // site, so handlers that produce further events are themselves
        // responsible for dispatching them.
        if (nextInLine.DomainEvents.Count > 0)
            await dispatcher.DispatchAsync(
                nextInLine.DomainEvents.ToList(), cancellationToken);
    }
}