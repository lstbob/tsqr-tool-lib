using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// When a reservation is pickup-confirmed, hold the InventoryItem for that
/// pickup by transitioning it from <see cref="ItemStatus.Available"/> to
/// <see cref="ItemStatus.Reserved"/>. Idempotent: if the item is already in any
/// non-Available state the handler does nothing - the hold invariant is already
/// satisfied or unreachable (see <see cref="NextInLineNotificationHandler"/>).
/// </summary>
public class PickupReminderHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IDomainEventHandler<PickupReminderEvent>
{
    public async Task HandleAsync(
        PickupReminderEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(domainEvent.ItemId, cancellationToken);
        if (item is null)
            return;

        if (item.Status != ItemStatus.Available)
            return;

        var reserveResult = item.Reserve();
        if (reserveResult.IsFailure)
            return;

        inventoryRepository.Update(item);
    }
}