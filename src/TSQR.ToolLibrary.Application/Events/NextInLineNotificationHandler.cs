namespace TSQR.ToolLibrary.Application.Events;

public class NextInLineNotificationHandler : IDomainEventHandler<NextInLineNotificationEvent>
{
    public Task HandleAsync(NextInLineNotificationEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
