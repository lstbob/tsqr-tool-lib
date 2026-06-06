namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record CompleteRepairCommand(MaintenanceRecordId RecordId, InventoryItemId ItemId, MemberId CompletedById, Condition NewCondition) : IRequest;

public class CompleteRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MaintenanceRecord, MaintenanceRecordId> maintenanceRepository)
    : IRequestHandler<CompleteRepairCommand>
{
    public async Task Handle(CompleteRepairCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken)
            ?? throw new InvalidOperationException("Inventory item not found.");

        var record = await maintenanceRepository.GetByIdAsync(request.RecordId, cancellationToken)
            ?? throw new InvalidOperationException("Maintenance record not found.");

        record.StartWork();
        record.Complete(request.CompletedById, request.NewCondition);
        item.CompleteRepair(request.NewCondition);

        await maintenanceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
