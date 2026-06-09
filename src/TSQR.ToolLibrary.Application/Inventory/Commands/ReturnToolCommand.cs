namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record ReturnToolCommand(InventoryItemId ItemId, Condition ReturnedCondition);

public class ReturnToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IReservationRepository reservationRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<ReturnToolCommand, Result>
{
    public async Task<Result> ExecuteAsync(ReturnToolCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var returnResult = item.Return(command.ReturnedCondition);
        if (returnResult.IsFailure)
            return returnResult.Error;

        var allReservations = await reservationRepository.GetByItemIdAsync(command.ItemId, cancellationToken);
        var queueService = new ReservationQueueService();
        var nextInLine = queueService.GetNextInLine(allReservations);

        if (nextInLine is not null)
        {
            nextInLine.AddDomainEvent(new NextInLineNotificationEvent(
                nextInLine.Id, nextInLine.ItemId, nextInLine.MemberId, "Tool has been returned and is now available."));
        }

        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var allEvents = new List<IDomainEvent>();
        allEvents.AddRange(item.DomainEvents);
        if (nextInLine is not null)
            allEvents.AddRange(nextInLine.DomainEvents);
        await eventDispatcher.DispatchAsync(allEvents, cancellationToken);
        item.ClearDomainEvents();
        nextInLine?.ClearDomainEvents();

        return Result.Success();
    }
}
