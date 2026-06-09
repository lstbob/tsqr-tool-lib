namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record MarkToolAsLostCommand(InventoryItemId ItemId, MemberId ReporterId);

public class MarkToolAsLostCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<MarkToolAsLostCommand, Result>
{
    public async Task<Result> ExecuteAsync(MarkToolAsLostCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var lostResult = item.MarkAsLost(command.ReporterId);
        if (lostResult.IsFailure)
            return lostResult.Error;

        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(item.DomainEvents, cancellationToken);
        item.ClearDomainEvents();

        return Result.Success();
    }
}
