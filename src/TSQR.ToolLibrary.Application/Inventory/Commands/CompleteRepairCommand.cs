namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record CompleteRepairCommand(InventoryItemId ItemId, MemberId CompletedById, Condition NewCondition);

public class CompleteRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<CompleteRepairCommand, Result>
{
    public async Task<Result> ExecuteAsync(CompleteRepairCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var repairResult = item.CompleteRepair(command.CompletedById, command.NewCondition);
        if (repairResult.IsFailure)
            return repairResult.Error;

        inventoryRepository.Update(item);

        await orchestrator.SaveEntitiesAsync([item], cancellationToken);

        return Result.Success();
    }
}
