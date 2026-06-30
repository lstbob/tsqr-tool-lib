using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record CompleteReservationCommand(ReservationId ReservationId);

public class CompleteReservationCommandHandler(
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<CompleteReservationCommand, Result>
{
    public async Task<Result> ExecuteAsync(CompleteReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
            return new NotFoundError(nameof(command.ReservationId), "Reservation not found.");

        var completeResult = reservation.Complete();
        if (completeResult.IsFailure)
            return completeResult.Error;

        reservationRepository.Update(reservation);
        await orchestrator.SaveEntitiesAsync(reservation, cancellationToken);

        return Result.Success();
    }
}
