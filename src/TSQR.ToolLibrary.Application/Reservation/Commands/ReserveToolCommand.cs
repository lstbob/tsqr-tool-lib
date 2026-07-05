using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ReserveToolCommand(
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate,
    int CommunityId = 1);

public class ReserveToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<ReserveToolCommand, Result<ReservationId>>
{
    public async Task<Result<ReservationId>> ExecuteAsync(ReserveToolCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var reserveResult = item.Reserve(command.ReservationDate, command.MemberId);
        if (reserveResult.IsFailure)
            return reserveResult.Error;

        var reservationResult = ReservationAgg.Create(
            command.ItemId,
            command.MemberId,
            command.ReservationDate,
            command.CommunityId);

        if (reservationResult.IsFailure)
            return reservationResult.Error;

        var reservation = reservationResult.Value;
        await reservationRepository.AddAsync(reservation, cancellationToken);
        inventoryRepository.Update(item);

        await orchestrator.SaveEntitiesAsync([reservation, item], cancellationToken);

        return reservation.Id;
    }
}
