using ReservationAgg = TSQR.ToolLibrary.Domain.Aggregates.ReservationAggregate.Reservation;

namespace TSQR.ToolLibrary.Application.Reservation.Commands;

public record CancelReservationCommand(ReservationId ReservationId);

public class CancelReservationCommandHandler(
    IReservationRepository reservationRepository,
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

        var allReservations = await reservationRepository.GetByItemIdAsync(reservation.ItemId, cancellationToken);
        var queueService = new ReservationQueueService();
        var nextInLine = queueService.GetNextInLine(allReservations);

        if (nextInLine is not null)
        {
            nextInLine.AddDomainEvent(new NextInLineNotificationEvent(
                nextInLine.Id, nextInLine.ItemId, nextInLine.MemberId, "Reservation was cancelled by the previous member."));
        }

        item?.ClearReservation();

        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var allEvents = new List<IDomainEvent>();
        allEvents.AddRange(reservation.DomainEvents);
        if (nextInLine is not null)
            allEvents.AddRange(nextInLine.DomainEvents);
        await eventDispatcher.DispatchAsync(allEvents, cancellationToken);
        reservation.ClearDomainEvents();
        nextInLine?.ClearDomainEvents();

        return Result.Success();
    }
}
