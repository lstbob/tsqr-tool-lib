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
    DomainEventOrchestrator orchestrator)
    : IInteractor<RegisterToolCommand, Result<ToolId>>
{
    public async Task<Result<ToolId>> ExecuteAsync(RegisterToolCommand command, CancellationToken cancellationToken)
    {
        // Tool.Register raises ToolRegisteredEvent inside the aggregate factory,
        // so the application layer does not raise domain events itself.
        var toolResult = ToolAgg.Register(
            command.OwnerId,
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

        var inventoryResult = InventoryItem.Create(
            tool.Id,
            command.OwnerId,
            DateTime.UtcNow,
            command.SerialNumber,
            command.InitialCondition);

        if (inventoryResult.IsFailure)
            return inventoryResult.Error;

        await inventoryRepository.AddAsync(inventoryResult.Value, cancellationToken);
        await orchestrator.SaveEntitiesAsync([tool, inventoryResult.Value], cancellationToken);

        return tool.Id;
    }
}
