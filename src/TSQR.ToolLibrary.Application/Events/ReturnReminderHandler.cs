namespace TSQR.ToolLibrary.Application.Events;

public class ReturnReminderHandler : IDomainEventHandler<ReturnReminderEvent>
{
    public Task HandleAsync(ReturnReminderEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
