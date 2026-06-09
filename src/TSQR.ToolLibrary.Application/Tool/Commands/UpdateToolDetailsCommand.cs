using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;

namespace TSQR.ToolLibrary.Application.Tool.Commands;

public record UpdateToolDetailsCommand(
    ToolId ToolId,
    string Model,
    string Description,
    Manufacturer Manufacturer,
    ToolType Type,
    AmortizationRate AmortizationRate,
    string? Metadata = null);

public class UpdateToolDetailsCommandHandler(
    IRepository<ToolAgg, ToolId> toolRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<UpdateToolDetailsCommand, Result>
{
    public async Task<Result> ExecuteAsync(UpdateToolDetailsCommand command, CancellationToken cancellationToken)
    {
        var tool = await toolRepository.GetByIdAsync(command.ToolId, cancellationToken);
        if (tool is null)
            return new NotFoundError(nameof(command.ToolId), "Tool not found.");

        var updateResult = tool.UpdateToolDetails(
            command.Model,
            command.Description,
            command.Manufacturer,
            command.Type,
            command.AmortizationRate,
            command.Metadata);

        if (updateResult.IsFailure)
            return updateResult.Error;

        await toolRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(tool.DomainEvents, cancellationToken);
        tool.ClearDomainEvents();

        return Result.Success();
    }
}
