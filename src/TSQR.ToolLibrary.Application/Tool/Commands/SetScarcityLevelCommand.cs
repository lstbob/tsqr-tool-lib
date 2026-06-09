using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;

namespace TSQR.ToolLibrary.Application.Tool.Commands;

public record SetScarcityLevelCommand(ToolId ToolId, LocationId LocationId, ScarcityLevel Level);

public class SetScarcityLevelCommandHandler(
    IRepository<ToolAgg, ToolId> toolRepository,
    IDomainEventDispatcher eventDispatcher)
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

        await toolRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(tool.DomainEvents, cancellationToken);
        tool.ClearDomainEvents();

        return Result.Success();
    }
}
