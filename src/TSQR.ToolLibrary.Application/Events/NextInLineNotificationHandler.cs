using TSQR.ToolLibrary.Domain.Aggregates.InventoryAggregate;
using TSQR.ToolLibrary.Domain.Events;

namespace TSQR.ToolLibrary.Application.Events;

/// <summary>
/// When a member is promoted to next-in-line for an item, hold the InventoryItem
/// for them by transitioning it from <see cref="ItemStatus.Available"/> to
/// <see cref="ItemStatus.Reserved"/>. Idempotent: if the item is already in any
/// non-Available state (Reserved by an earlier promotion, Loaned, UnderMaintenance,
/// Lost) the handler does nothing - the cross-aggregate invariant "the item is
/// held for the next-in-line member when reachable" is already satisfied or
/// unreachable.
/// </summary>
public class NextInLineNotificationHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IDomainEventHandler<NextInLineNotificationEvent>
{
    public async Task HandleAsync(
        NextInLineNotificationEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(domainEvent.ItemId, cancellationToken);
        if (item is null)
            return;

        // Only Available items can be Reserved. Any other state means either
        // (a) another handler already held it, or (b) the item is genuinely
        // unavailable (loaned, under maintenance, lost). Either way, no-op.
        if (item.Status != ItemStatus.Available)
            return;

        var reserveResult = item.Reserve();
        if (reserveResult.IsFailure)
            return;

        inventoryRepository.Update(item);
    }
}