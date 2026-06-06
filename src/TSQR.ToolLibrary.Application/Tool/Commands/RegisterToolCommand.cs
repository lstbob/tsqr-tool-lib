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
    string? Metadata = null) : IRequest<ToolId>;

public class RegisterToolCommandHandler(
    IRepository<ToolAgg.Tool, ToolId> toolRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IRequestHandler<RegisterToolCommand, ToolId>
{
    public async Task<ToolId> Handle(RegisterToolCommand request, CancellationToken cancellationToken)
    {
        var tool = ToolAgg.Tool.Create(
            request.Model,
            request.Description,
            request.Manufacturer,
            request.Type,
            request.AmortizationRate,
            request.Metadata);

        await toolRepository.AddAsync(tool, cancellationToken);
        await toolRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var inventoryItem = InventoryItem.Create(
            tool.Id,
            request.OwnerId,
            DateTime.UtcNow,
            request.SerialNumber,
            request.InitialCondition);

        await inventoryRepository.AddAsync(inventoryItem, cancellationToken);
        await inventoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        tool.AddDomainEvent(new ToolRegisteredEvent(tool.Id, request.OwnerId, request.Model, request.Type));

        return tool.Id;
    }
}
