using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record CancelReservationCommand(ReservationId ReservationId);

public class CancelReservationCommandHandler(
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<CancelReservationCommand, Result>
{
    public async Task<Result> ExecuteAsync(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
            return new NotFoundError(nameof(command.ReservationId), "Reservation not found.");

        var item = await inventoryRepository.GetByIdAsync(reservation.ItemId, cancellationToken);

        var cancelResult = reservation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult.Error;

        item?.ClearReservation();

        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(reservation.DomainEvents, cancellationToken);
        reservation.ClearDomainEvents();

        return Result.Success();
    }
}
