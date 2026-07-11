namespace TSQR.ToolLibrary.Application.Events;

public class LoanCreatedDomainEventHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository)
    : IDomainEventHandler<LoanCreatedDomainEvent>
{
    public async Task HandleAsync(LoanCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var item = await inventoryRepository.GetByIdAsync(domainEvent.ItemId, cancellationToken)
            ?? throw new InvalidOperationException($"InventoryItem {domainEvent.ItemId} not found for loan side-effect.");

        var loanResult = item.Loan(domainEvent.MemberId);
        if (loanResult.IsFailure)
            throw new InvalidOperationException($"InventoryItem.Loan failed: {loanResult.Error}");

        inventoryRepository.Update(item);
    }
}
