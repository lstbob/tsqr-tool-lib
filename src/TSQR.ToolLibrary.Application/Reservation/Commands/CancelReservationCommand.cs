using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record CancelReservationCommand(ReservationId ReservationId) : IRequest<Result>;

public class CancelReservationCommandHandler(
    IRepository<ReservationAgg.Reservation, ReservationId> reservationRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IRequestHandler<CancelReservationCommand, Result>
{
    public async Task<Result> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);
        if (reservation is null)
            return new NotFoundError(nameof(request.ReservationId), "Reservation not found.");

        var item = await inventoryRepository.GetByIdAsync(reservation.ItemId, cancellationToken);

        var cancelResult = reservation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult.Error;

        item?.ClearReservation();

        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
