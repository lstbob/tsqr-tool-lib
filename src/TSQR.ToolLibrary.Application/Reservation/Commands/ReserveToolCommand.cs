using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ReserveToolCommand(
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate,
    int CommunityId = 1);

public class ReserveToolCommandHandler(
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<ReserveToolCommand, Result<ReservationId>>
{
    public async Task<Result<ReservationId>> ExecuteAsync(ReserveToolCommand command, CancellationToken cancellationToken)
    {
        var reservationResult = ReservationAgg.Create(
            command.ItemId,
            command.MemberId,
            command.ReservationDate,
            command.CommunityId);

        if (reservationResult.IsFailure)
            return reservationResult.Error;

        var reservation = reservationResult.Value;
        await reservationRepository.AddAsync(reservation, cancellationToken);
        await orchestrator.SaveEntitiesAsync(reservation, cancellationToken);

        return reservation.Id;
    }
}
