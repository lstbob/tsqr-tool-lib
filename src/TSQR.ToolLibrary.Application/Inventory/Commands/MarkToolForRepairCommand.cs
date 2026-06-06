namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record MarkToolForRepairCommand(InventoryItemId ItemId, MemberId ReportedById, string Description) : IRequest<Result>;

public class MarkToolForRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MaintenanceRecord, MaintenanceRecordId> maintenanceRepository)
    : IRequestHandler<MarkToolForRepairCommand, Result>
{
    public async Task<Result> Handle(MarkToolForRepairCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(request.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(request.ItemId), "Inventory item not found.");

        var markResult = item.MarkForRepair();
        if (markResult.IsFailure)
            return markResult.Error;

        var recordResult = MaintenanceRecord.Create(request.ItemId, request.ReportedById, request.Description);
        if (recordResult.IsFailure)
            return recordResult.Error;

        await maintenanceRepository.AddAsync(recordResult.Value, cancellationToken);

        item.AddDomainEvent(new ToolMarkedForRepairEvent(request.ItemId, request.ReportedById, request.Description));

        await maintenanceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
