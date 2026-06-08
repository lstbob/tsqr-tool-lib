using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate;

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
    string? Metadata = null) : IRequest<Result<ToolId>>;

public class RegisterToolCommandHandler(
    IRepository<ToolAgg.Tool, ToolId> toolRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IRequestHandler<RegisterToolCommand, Result<ToolId>>
{
    public async Task<Result<ToolId>> Handle(RegisterToolCommand request, CancellationToken cancellationToken)
    {
        var toolResult = ToolAgg.Tool.Create(
            request.Model,
            request.Description,
            request.Manufacturer,
            request.Type,
            request.AmortizationRate,
            request.Metadata);

        if (toolResult.IsFailure)
            return toolResult.Error;

        var tool = toolResult.Value;
        await toolRepository.AddAsync(tool, cancellationToken);
        await toolRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var inventoryResult = InventoryItem.Create(
            tool.Id,
            request.OwnerId,
            DateTime.UtcNow,
            request.SerialNumber,
            request.InitialCondition);

        if (inventoryResult.IsFailure)
            return inventoryResult.Error;

        await inventoryRepository.AddAsync(inventoryResult.Value, cancellationToken);
        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        tool.AddDomainEvent(new ToolRegisteredEvent(tool.Id, request.OwnerId, request.Model, request.Type));

        return tool.Id;
    }
}
