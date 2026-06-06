namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record CompleteRepairCommand(MaintenanceRecordId RecordId, InventoryItemId ItemId, MemberId CompletedById, Condition NewCondition) : IRequest<Result>;

public class CompleteRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MaintenanceRecord, MaintenanceRecordId> maintenanceRepository)
    : IRequestHandler<CompleteRepairCommand, Result>
{
    public async Task<Result> Handle(CompleteRepairCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(request.ItemId), "Inventory item not found.");

        var record = await maintenanceRepository.GetByIdAsync(request.RecordId, cancellationToken);
        if (record is null)
            return new NotFoundError(nameof(request.RecordId), "Maintenance record not found.");

        var startResult = record.StartWork();
        if (startResult.IsFailure) return startResult.Error;

        var completeResult = record.Complete(request.CompletedById, request.NewCondition);
        if (completeResult.IsFailure) return completeResult.Error;

        var repairResult = item.CompleteRepair(request.NewCondition);
        if (repairResult.IsFailure) return repairResult.Error;

        await maintenanceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
