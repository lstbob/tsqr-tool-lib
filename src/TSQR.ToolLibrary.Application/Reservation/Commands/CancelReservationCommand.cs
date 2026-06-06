using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record CancelReservationCommand(ReservationId ReservationId) : IRequest;

public class CancelReservationCommandHandler(
    IRepository<ReservationAgg.Reservation, ReservationId> reservationRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IRequestHandler<CancelReservationCommand>
{
    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new InvalidOperationException("Reservation not found.");

        var item = await inventoryRepository.GetByIdAsync(reservation.ItemId, cancellationToken);

        reservation.Cancel();
        item?.ClearReservation();

        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
