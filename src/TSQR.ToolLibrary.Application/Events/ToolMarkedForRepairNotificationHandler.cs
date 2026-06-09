namespace TSQR.ToolLibrary.Application.Events;

public class ToolMarkedForRepairNotificationHandler : IDomainEventHandler<ToolMarkedForRepairEvent>
{
    public Task HandleAsync(ToolMarkedForRepairEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
