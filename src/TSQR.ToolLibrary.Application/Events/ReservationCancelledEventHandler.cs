namespace TSQR.ToolLibrary.Application.Events;

public class ReservationCancelledEventHandler(
    IReservationRepository reservationRepository)
    : IDomainEventHandler<ReservationCancelledEvent>
{
    public async Task HandleAsync(ReservationCancelledEvent domainEvent, CancellationToken cancellationToken)
    {
        var allReservations = await reservationRepository.GetByItemIdAsync(domainEvent.ItemId, cancellationToken);
        var queueService = new ReservationQueueService();
        var nextInLine = queueService.GetNextInLine(allReservations);

        if (nextInLine is null)
            return;

        nextInLine.NotifyNextInLine("A prior reservation was cancelled. The tool is now available for you.");
        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
