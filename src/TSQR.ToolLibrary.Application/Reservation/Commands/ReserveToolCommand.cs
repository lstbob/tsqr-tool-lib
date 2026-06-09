using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record ReserveToolCommand(
    InventoryItemId ItemId,
    MemberId MemberId,
    DateTime ReservationDate);

public class ReserveToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<ReservationAgg, ReservationId> reservationRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<ReserveToolCommand, Result<ReservationId>>
{
    public async Task<Result<ReservationId>> ExecuteAsync(ReserveToolCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var maxReservationDate = DateTime.UtcNow.AddDays(28);
        var expiryDate = command.ReservationDate.AddDays(14);

        if (command.ReservationDate > maxReservationDate)
            return new DomainError(nameof(command.ReservationDate), "Reservations can only be made up to 28 days in advance.");

        var reserveResult = item.Reserve(command.ReservationDate, command.MemberId);
        if (reserveResult.IsFailure)
            return reserveResult.Error;

        var reservationResult = ReservationAgg.Create(
            command.ItemId,
            command.MemberId,
            command.ReservationDate,
            expiryDate,
            1);

        if (reservationResult.IsFailure)
            return reservationResult.Error;

        var reservation = reservationResult.Value;
        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(reservation.DomainEvents, cancellationToken);
        reservation.ClearDomainEvents();

        return reservation.Id;
    }
}
