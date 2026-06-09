using LoanAgg = TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate.Loan;

namespace TSQR.ToolLibrary.Application.Loan.Commands;

public record MarkLoanAsNotReturnedCommand(LoanId LoanId, InventoryItemId ItemId);

public class MarkLoanAsNotReturnedCommandHandler(
    IRepository<LoanAgg, LoanId> loanRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<MarkLoanAsNotReturnedCommand, Result>
{
    public async Task<Result> ExecuteAsync(MarkLoanAsNotReturnedCommand command, CancellationToken cancellationToken)
    {
        var loan = await loanRepository.GetByIdAsync(command.LoanId, cancellationToken);
        if (loan is null)
            return new NotFoundError(nameof(command.LoanId), "Loan not found.");

        if (loan.Status != LoanStatus.Active)
            return new DomainError(nameof(loan.Status), "Only active loans can be marked as not returned.");

        var endResult = loan.EndLoan(DateTime.UtcNow);
        if (endResult.IsFailure)
            return endResult.Error;

        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is not null)
        {
            item.AddDomainEvent(new LoanOverdueDomainEvent(loan.Id, command.ItemId, DateTime.UtcNow - loan.DueDate));
        }

        await loanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var allEvents = new List<IDomainEvent>();
        allEvents.AddRange(loan.DomainEvents);
        if (item is not null)
            allEvents.AddRange(item.DomainEvents);
        await eventDispatcher.DispatchAsync(allEvents, cancellationToken);
        loan.ClearDomainEvents();
        item?.ClearDomainEvents();

        return Result.Success();
    }
}
