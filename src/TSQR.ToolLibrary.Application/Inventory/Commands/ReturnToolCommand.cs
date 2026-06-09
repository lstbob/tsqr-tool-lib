namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record ReturnToolCommand(InventoryItemId ItemId, Condition ReturnedCondition);

public class ReturnToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
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

        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(item.DomainEvents, cancellationToken);
        item.ClearDomainEvents();

        return Result.Success();
    }
}
