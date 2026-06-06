using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ConfirmPickupCommand(ReservationId ReservationId) : IRequest<Result>;

public class ConfirmPickupCommandHandler(IRepository<ReservationAgg.Reservation, ReservationId> reservationRepository)
    : IRequestHandler<ConfirmPickupCommand, Result>
{
    public async Task<Result> Handle(ConfirmPickupCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);
        if (reservation is null)
            return new NotFoundError(nameof(request.ReservationId), "Reservation not found.");

        var confirmResult = reservation.ConfirmPickup();
        if (confirmResult.IsFailure)
            return confirmResult.Error;

        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
