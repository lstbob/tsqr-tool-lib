namespace TSQR.ToolLibrary.Application.Events;

public class ReservationCreatedDomainEventHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IDomainEventHandler<ReservationCreatedDomainEvent>
{
    public async Task HandleAsync(ReservationCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(domainEvent.ItemId, cancellationToken)
            ?? throw new InvalidOperationException($"InventoryItem {domainEvent.ItemId} not found for reservation side-effect.");

        var reserveResult = item.Reserve();
        if (reserveResult.IsFailure)
            throw new InvalidOperationException($"InventoryItem.Reserve failed: {reserveResult.Error}");

        inventoryRepository.Update(item);
    }
}
