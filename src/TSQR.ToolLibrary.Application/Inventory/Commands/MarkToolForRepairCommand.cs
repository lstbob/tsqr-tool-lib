namespace TSQR.ToolLibrary.Application.Inventory.Commands;

public record MarkToolForRepairCommand(InventoryItemId ItemId, MemberId ReportedById, string Description);

public class MarkToolForRepairCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MaintenanceRecord, MaintenanceRecordId> maintenanceRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<MarkToolForRepairCommand, Result>
{
    public async Task<Result> ExecuteAsync(MarkToolForRepairCommand command, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var markResult = item.MarkForRepair();
        if (markResult.IsFailure)
            return markResult.Error;

        var recordResult = MaintenanceRecord.Create(command.ItemId, command.ReportedById, command.Description);
        if (recordResult.IsFailure)
            return recordResult.Error;

        await maintenanceRepository.AddAsync(recordResult.Value, cancellationToken);

        item.AddDomainEvent(new ToolMarkedForRepairEvent(command.ItemId, command.ReportedById, command.Description));

        await maintenanceRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(item.DomainEvents, cancellationToken);
        item.ClearDomainEvents();

        return Result.Success();
    }
}
