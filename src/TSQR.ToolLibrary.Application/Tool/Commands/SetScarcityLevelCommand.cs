using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;

namespace TSQR.ToolLibrary.Application.Tool.Commands;

public record SetScarcityLevelCommand(ToolId ToolId, LocationId LocationId, ScarcityLevel Level);

public class SetScarcityLevelCommandHandler(
    IRepository<ToolAgg, ToolId> toolRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<SetScarcityLevelCommand, Result>
{
    public async Task<Result> ExecuteAsync(SetScarcityLevelCommand command, CancellationToken cancellationToken)
    {
        var tool = await toolRepository.GetByIdAsync(command.ToolId, cancellationToken);
        if (tool is null)
            return new NotFoundError(nameof(command.ToolId), "Tool not found.");

        var setResult = tool.SetScarcityLevel(command.LocationId, command.Level);
        if (setResult.IsFailure)
            return setResult.Error;

        toolRepository.Update(tool);
        await orchestrator.SaveEntitiesAsync(tool, cancellationToken);

        return Result.Success();
    }
}
