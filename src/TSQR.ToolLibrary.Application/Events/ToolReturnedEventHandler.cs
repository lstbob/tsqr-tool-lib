namespace TSQR.ToolLibrary.Application.Events;

public class ToolReturnedEventHandler(
    IReservationRepository reservationRepository)
    : IDomainEventHandler<ToolReturnedEvent>
{
    public async Task HandleAsync(ToolReturnedEvent domainEvent, CancellationToken cancellationToken)
    {
        var allReservations = await reservationRepository.GetByItemIdAsync(domainEvent.ItemId, cancellationToken);
        var queueService = new ReservationQueueService();
        var nextInLine = queueService.GetNextInLine(allReservations);

        if (nextInLine is null)
            return;

        nextInLine.NotifyNextInLine("The tool has been returned and is now available for you.");
        await reservationRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
