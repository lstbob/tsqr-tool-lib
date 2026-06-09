using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;
using LoanAgg = TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate.Loan;

namespace TSQR.ToolLibrary.Application.Loan.Commands;

public record LoanToolCommand(InventoryItemId ItemId, MemberId MemberId);

public class LoanToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MemberAgg, MemberId> memberRepository,
    IRepository<LoanAgg, LoanId> loanRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<LoanToolCommand, Result>
{
    public async Task<Result> ExecuteAsync(LoanToolCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        if (!member.IsEligibleToBorrow())
            return new DomainError(nameof(member.Status), "Member is not eligible to borrow tools.");

        var item = await inventoryRepository.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(command.ItemId), "Inventory item not found.");

        var dueDate = DateTime.UtcNow.AddDays(7);

        var loanResult = LoanAgg.Create(command.MemberId, DateTime.UtcNow, LoanStatus.Active, dueDate, command.ItemId);
        if (loanResult.IsFailure)
            return loanResult.Error;

        var loan = loanResult.Value;
        await loanRepository.AddAsync(loan, cancellationToken);

        var loanItemResult = item.Loan(command.MemberId);
        if (loanItemResult.IsFailure)
            return loanItemResult.Error;

        await loanRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var allEvents = new List<IDomainEvent>();
        allEvents.AddRange(loan.DomainEvents);
        allEvents.AddRange(item.DomainEvents);
        await eventDispatcher.DispatchAsync(allEvents, cancellationToken);
        loan.ClearDomainEvents();
        item.ClearDomainEvents();

        return Result.Success();
    }
}
