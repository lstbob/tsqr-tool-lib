namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record MarkToolForRepairCommand(InventoryItemId ItemId, MemberId ReportedById, string Description);

public class MarkToolForRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<MarkToolForRepairCommand, Result>
{
    public async Task<Result> ExecuteAsync(MarkToolForRepairCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var markResult = item.MarkForRepair(command.ReportedById, command.Description);
        if (markResult.IsFailure)
            return markResult.Error;

        inventoryRepository.Update(item);

        await orchestrator.SaveEntitiesAsync([item], cancellationToken);

        return Result.Success();
    }
}
