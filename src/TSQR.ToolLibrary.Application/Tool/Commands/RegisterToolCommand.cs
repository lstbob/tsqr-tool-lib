using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;

namespace TSQR.ToolLibrary.Application.Tool.Commands;

public record RegisterToolCommand(
    string Model,
    string Description,
    Manufacturer Manufacturer,
    ToolType Type,
    AmortizationRate AmortizationRate,
    MemberId OwnerId,
    string SerialNumber,
    Condition InitialCondition,
    string? Metadata = null);

public class RegisterToolCommandHandler(
    IRepository<ToolAgg, ToolId> toolRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<RegisterToolCommand, Result<ToolId>>
{
    public async Task<Result<ToolId>> ExecuteAsync(RegisterToolCommand command, CancellationToken cancellationToken)
    {
        var toolResult = ToolAgg.Create(
            command.Model,
            command.Description,
            command.Manufacturer,
            command.Type,
            command.AmortizationRate,
            command.Metadata);

        if (toolResult.IsFailure)
            return toolResult.Error;

        var tool = toolResult.Value;
        await toolRepository.AddAsync(tool, cancellationToken);
        await toolRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var inventoryResult = InventoryItem.Create(
            tool.Id,
            command.OwnerId,
            DateTime.UtcNow,
            command.SerialNumber,
            command.InitialCondition);

        if (inventoryResult.IsFailure)
            return inventoryResult.Error;

        await inventoryRepository.AddAsync(inventoryResult.Value, cancellationToken);
        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        tool.AddDomainEvent(new ToolRegisteredEvent(tool.Id, command.OwnerId, command.Model, command.Type));
        await eventDispatcher.DispatchAsync(tool.DomainEvents, cancellationToken);
        tool.ClearDomainEvents();

        return tool.Id;
    }
}
