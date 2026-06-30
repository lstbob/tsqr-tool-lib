namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record CompleteRepairCommand(MaintenanceRecordId RecordId, InventoryItemId ItemId, MemberId CompletedById, Condition NewCondition);

public class CompleteRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MaintenanceRecord, MaintenanceRecordId> maintenanceRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<CompleteRepairCommand, Result>
{
    public async Task<Result> ExecuteAsync(CompleteRepairCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var record = await maintenanceRepository.GetByIdAsync(command.RecordId, cancellationToken);
        if (record is null)
            return new NotFoundError(nameof(command.RecordId), "Maintenance record not found.");

        var startResult = record.StartWork();
        if (startResult.IsFailure) return startResult.Error;

        var completeResult = record.Complete(command.CompletedById, command.NewCondition);
        if (completeResult.IsFailure) return completeResult.Error;

        var repairResult = item.CompleteRepair(command.NewCondition);
        if (repairResult.IsFailure) return repairResult.Error;

        maintenanceRepository.Update(record);
        inventoryRepository.Update(item);

        await orchestrator.SaveEntitiesAsync([record, item], cancellationToken);

        return Result.Success();
    }
}
