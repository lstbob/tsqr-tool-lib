namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record MarkToolForRepairCommand(InventoryItemId ItemId, MemberId ReportedById, string Description) : IRequest;

public class MarkToolForRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MaintenanceRecord, MaintenanceRecordId> maintenanceRepository)
    : IRequestHandler<MarkToolForRepairCommand>
{
    public async Task Handle(MarkToolForRepairCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken)
            ?? throw new InvalidOperationException("Inventory item not found.");

        item.MarkForRepair();

        var record = MaintenanceRecord.Create(request.ItemId, request.ReportedById, request.Description);
        await maintenanceRepository.AddAsync(record, cancellationToken);

        item.AddDomainEvent(new ToolMarkedForRepairEvent(request.ItemId, request.ReportedById, request.Description));

        await maintenanceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
