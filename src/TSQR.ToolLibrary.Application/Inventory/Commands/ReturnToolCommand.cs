namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record ReturnToolCommand(InventoryItemId ItemId, Condition ReturnedCondition) : IRequest;

public class ReturnToolCommandHandler(IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IRequestHandler<ReturnToolCommand>
{
    public async Task Handle(ReturnToolCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken)
            ?? throw new InvalidOperationException("Inventory item not found.");

        item.Return(request.ReturnedCondition);
        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
