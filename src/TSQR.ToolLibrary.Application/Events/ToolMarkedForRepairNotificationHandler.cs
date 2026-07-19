using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// When an InventoryItem is marked for repair, the item cannot be loaned or
/// picked up, so every non-terminal reservation on it must be cancelled. The
/// handler cancels all <see cref="ReservationStatus.Pending"/> and
/// <see cref="ReservationStatus.Confirmed"/> reservations, persists each mutation
/// via the repository, then explicitly dispatches the resulting
/// <see cref="ReservationCancelledEvent"/>s within the same unit-of-work
/// transaction. Each dispatched event will run
/// <see cref="ReservationCancelledEventHandler"/>, which shifts the queue
/// positions behind the cancelled slot and notifies (and reserves the item for)
/// the next-in-line member.
/// </summary>
public class ToolMarkedForRepairNotificationHandler(
    IReservationRepository reservationRepository,
    IDomainEventDispatcher dispatcher)
    : IDomainEventHandler<ToolMarkedForRepairEvent>
{
    public async Task HandleAsync(
        ToolMarkedForRepairEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var reservations = await reservationRepository.GetByItemIdAsync(
            domainEvent.ItemId, cancellationToken);

        var cancellable = reservations
            .Where(r => r.Status == ReservationStatus.Pending
                     || r.Status == ReservationStatus.Confirmed)
            .ToList();

        if (cancellable.Count == 0)
            return;

        var cascadedEvents = new List<IDomainEvent>();

        foreach (var reservation in cancellable)
        {
            var cancelResult = reservation.Cancel();
            if (cancelResult.IsFailure)
                // The pre-filter on Status above guarantees Cancel() succeeds;
                // a failure here would mean a concurrent mutation. Skip and let
                // the queue-shift logic in the downstream handler reconcile.
                continue;

            reservationRepository.Update(reservation);
            cascadedEvents.AddRange(reservation.DomainEvents);
        }

        // Explicitly fan out the new events so the cancellation chain (queue
        // shift + next-in-line notification + item hold) runs in the same
        // transaction. The single-pass DomainEventDispatcher only iterates
        // events present at the call site, so handlers that produce further
        // events are themselves responsible for dispatching them.
        if (cascadedEvents.Count > 0)
            await dispatcher.DispatchAsync(cascadedEvents, cancellationToken);
    }
}