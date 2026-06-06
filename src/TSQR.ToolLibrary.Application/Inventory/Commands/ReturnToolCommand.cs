namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record ReturnToolCommand(InventoryItemId ItemId, Condition ReturnedCondition) : IRequest<Result>;

public class ReturnToolCommandHandler(IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IRequestHandler<ReturnToolCommand, Result>
{
    public async Task<Result> Handle(ReturnToolCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(request.ItemId), "Inventory item not found.");

        var returnResult = item.Return(request.ReturnedCondition);
        if (returnResult.IsFailure)
            return returnResult.Error;

        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
