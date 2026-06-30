namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record MarkToolAsLostCommand(InventoryItemId ItemId, MemberId ReporterId);

public class MarkToolAsLostCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    DomainEventOrchestrator orchestrator)
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

        inventoryRepository.Update(item);
        await orchestrator.SaveEntitiesAsync(item, cancellationToken);

        return Result.Success();
    }
}
