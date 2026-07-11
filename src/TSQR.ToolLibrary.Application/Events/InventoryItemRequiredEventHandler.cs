namespace TSQR.ToolLibrary.Application.Events;

public class InventoryItemRequiredEventHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IDomainEventHandler<InventoryItemRequiredEvent>
{
    public async Task HandleAsync(InventoryItemRequiredEvent domainEvent, CancellationToken cancellationToken)
    {
        var item = InventoryItem.Create(
            domainEvent.ToolId,
            domainEvent.OwnerId,
            DateTime.UtcNow,
            domainEvent.SerialNumber,
            domainEvent.InitialCondition,
            domainEvent.CommunityId);

        if (item.IsFailure)
            throw new InvalidOperationException($"InventoryItem.Create failed: {item.Error}");

        await inventoryRepository.AddAsync(item.Value, cancellationToken);
    }
}
