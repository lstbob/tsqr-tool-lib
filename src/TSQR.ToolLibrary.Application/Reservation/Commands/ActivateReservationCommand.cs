using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ActivateReservationCommand(ReservationId ReservationId);

public class ActivateReservationCommandHandler(
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<ActivateReservationCommand, Result>
{
    public async Task<Result> ExecuteAsync(ActivateReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
            return new NotFoundError(nameof(command.ReservationId), "Reservation not found.");

        var activateResult = reservation.Activate();
        if (activateResult.IsFailure)
            return activateResult.Error;

        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(reservation.DomainEvents, cancellationToken);
        reservation.ClearDomainEvents();

        return Result.Success();
    }
}
