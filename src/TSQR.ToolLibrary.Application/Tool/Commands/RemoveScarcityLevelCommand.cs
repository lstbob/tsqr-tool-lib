using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;

namespace TSQR.ToolLibrary.Application.Tool.Commands;

public record RemoveScarcityLevelCommand(ToolId ToolId, LocationId LocationId);

public class RemoveScarcityLevelCommandHandler(
    IRepository<ToolAgg, ToolId> toolRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<RemoveScarcityLevelCommand, Result>
{
    public async Task<Result> ExecuteAsync(RemoveScarcityLevelCommand command, CancellationToken cancellationToken)
    {
        var tool = await toolRepository.GetByIdAsync(command.ToolId, cancellationToken);
        if (tool is null)
            return new NotFoundError(nameof(command.ToolId), "Tool not found.");

        tool.RemoveScarcityLevel(command.LocationId);
        toolRepository.Update(tool);
        await orchestrator.SaveEntitiesAsync(tool, cancellationToken);

        return Result.Success();
    }
}
