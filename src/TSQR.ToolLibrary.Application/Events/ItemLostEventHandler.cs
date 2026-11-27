using TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// Closes the active loan of a lost item and cancels its open reservations.
/// </summary>
public class ItemLostEventHandler(
    ILoanRepository loanRepository,
    IReservationRepository reservationRepository)
    : IDomainEventHandler<ItemLostDomainEvent>
{
    public async ValueTask HandleAsync(ItemLostDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var loan = await loanRepository.GetActiveLoanByItemAsync(domainEvent.ItemId, cancellationToken);
        if (loan is not null && loan.MarkItemLost(domainEvent.OccurredOn).IsSuccess)
            loanRepository.Update(loan);

        var reservations = await reservationRepository.GetByItemIdAsync(domainEvent.ItemId, cancellationToken);
        foreach (var reservation in reservations.Where(r =>
                     r.Status is ReservationStatus.Pending or ReservationStatus.Confirmed or ReservationStatus.Active))
        {
            if (reservation.Cancel().IsSuccess)
                reservationRepository.Update(reservation);
        }
    }
}
