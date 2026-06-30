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
        reservationRepository.Update(nextInLine);

        // The mutation on nextInLine is committed by the outer
        // DomainEventOrchestrator.SaveEntitiesAsync as part of the originating
        // command's transaction. If the original command rolls back, this
        // side-effect rolls back too - preserving cross-aggregate invariants.
    }
}
