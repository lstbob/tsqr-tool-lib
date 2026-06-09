namespace TSQR.ToolLibrary.Application.Events;

public class PickupReminderHandler : IDomainEventHandler<PickupReminderEvent>
{
    public Task HandleAsync(PickupReminderEvent domainEvent, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
