using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ConfirmPickupCommand(ReservationId ReservationId) : IRequest;

public class ConfirmPickupCommandHandler(IRepository<ReservationAgg.Reservation, ReservationId> reservationRepository)
    : IRequestHandler<ConfirmPickupCommand>
{
    public async Task Handle(ConfirmPickupCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken)
            ?? throw new InvalidOperationException("Reservation not found.");

        reservation.ConfirmPickup();
        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
