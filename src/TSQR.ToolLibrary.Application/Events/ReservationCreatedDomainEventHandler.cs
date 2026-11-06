namespace TSQR.ToolLibrary.Application.Events;

public class ReservationCreatedDomainEventHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IDomainEventHandler<ReservationCreatedDomainEvent>
{
    public async ValueTask HandleAsync(ReservationCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(domainEvent.ItemId, cancellationToken)
            ?? throw new InvalidOperationException($"InventoryItem {domainEvent.ItemId} not found for reservation side-effect.");

        // The first reservation in an empty queue holds an Available item for
        // the member. Later reservations - and reservations made while the item
        // is loaned/reserved/under maintenance/lost - only join the queue.
        //
        // QueuePosition is captured by the factory before the repository
        // assigns the database id, so it is the reliable way to tell whether
        // this is the first reservation. The event's ReservationId still points
        // at the pre-assignment value (0) by dispatch time.
        if (domainEvent.QueuePosition > 1 || item.Status != ItemStatus.Available)
            return;

        var reserveResult = item.Reserve();
        if (reserveResult.IsFailure)
            throw new InvalidOperationException($"InventoryItem.Reserve failed: {reserveResult.Error}");

        inventoryRepository.Update(item);
    }
}
