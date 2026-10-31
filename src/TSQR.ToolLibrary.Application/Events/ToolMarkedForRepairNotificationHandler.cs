using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// When an InventoryItem is marked for repair, the item cannot be loaned or
/// picked up, so every non-terminal reservation on it must be cancelled. The
/// handler cancels all <see cref="ReservationStatus.Pending"/> and
/// <see cref="ReservationStatus.Confirmed"/> reservations and persists each
/// mutation via the repository. The orchestrator then drains the resulting
/// <see cref="ReservationCancelledEvent"/>s in a subsequent in-transaction
/// pass. Each one runs <see cref="ReservationCancelledEventHandler"/>, which
/// shifts the queue positions behind the cancelled slot and notifies (and
/// reserves the item for) the next-in-line member.
/// </summary>
public class ToolMarkedForRepairNotificationHandler(
    IReservationRepository reservationRepository)
    : IDomainEventHandler<ToolMarkedForRepairEvent>
{
    public async ValueTask HandleAsync(
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

        foreach (var reservation in cancellable)
        {
            var cancelResult = reservation.Cancel();
            if (cancelResult.IsFailure)
                // The pre-filter on Status above guarantees Cancel() succeeds;
                // a failure here would mean a concurrent mutation. Skip and let
                // the queue-shift logic in the downstream handler reconcile.
                continue;

            reservationRepository.Update(reservation);
        }

        // The orchestrator drains the ReservationCancelledEvents raised here
        // in subsequent passes, still inside the same transaction. That drives
        // queue shifting, next-in-line promotion, and the item hold.
    }
}