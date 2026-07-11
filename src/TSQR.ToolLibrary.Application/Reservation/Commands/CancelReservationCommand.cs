using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record CancelReservationCommand(ReservationId ReservationId);

public class CancelReservationCommandHandler(
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<CancelReservationCommand, Result>
{
    public async Task<Result> ExecuteAsync(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
            return new NotFoundError(nameof(command.ReservationId), "Reservation not found.");

        var cancelResult = reservation.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult.Error;

        reservationRepository.Update(reservation);
        await orchestrator.SaveEntitiesAsync(reservation, cancellationToken);

        return Result.Success();
    }
}
