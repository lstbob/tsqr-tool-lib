using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;
using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

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
    string? Metadata = null,
    int CommunityId = 1);

public class RegisterToolCommandHandler(
    IRepository<ToolAgg, ToolId> toolRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MemberAgg, MemberId> memberRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<RegisterToolCommand, Result<ToolId>>
{
    public async Task<Result<ToolId>> ExecuteAsync(RegisterToolCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.OwnerId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.OwnerId), "Owner member not found.");

        var toolResult = ToolAgg.Register(
            command.OwnerId,
            command.Model,
            command.Description,
            command.Manufacturer,
            command.Type,
            command.AmortizationRate,
            command.Metadata,
            command.CommunityId);

        if (toolResult.IsFailure)
            return toolResult.Error;

        var tool = toolResult.Value;
        await toolRepository.AddAsync(tool, cancellationToken);

        var inventoryResult = InventoryItem.Create(
            tool.Id,
            command.OwnerId,
            DateTime.UtcNow,
            command.SerialNumber,
            command.InitialCondition,
            command.CommunityId);

        if (inventoryResult.IsFailure)
            return inventoryResult.Error;

        await inventoryRepository.AddAsync(inventoryResult.Value, cancellationToken);
        await orchestrator.SaveEntitiesAsync([tool, inventoryResult.Value], cancellationToken);

        return tool.Id;
    }
}
