using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ConfirmPickupCommand(ReservationId ReservationId);

public class ConfirmPickupCommandHandler(
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<ConfirmPickupCommand, Result>
{
    public async Task<Result> ExecuteAsync(ConfirmPickupCommand command, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
            return new NotFoundError(nameof(command.ReservationId), "Reservation not found.");

        var confirmResult = reservation.ConfirmPickup();
        if (confirmResult.IsFailure)
            return confirmResult.Error;

        reservationRepository.Update(reservation);
        await orchestrator.SaveEntitiesAsync(reservation, cancellationToken);

        return Result.Success();
    }
}
