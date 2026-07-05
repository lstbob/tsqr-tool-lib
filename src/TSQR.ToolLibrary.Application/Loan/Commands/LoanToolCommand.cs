using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;
using LoanAgg = TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate.Loan;

namespace TSQR.ToolLibrary.Application.Loan.Commands;

public record LoanToolCommand(InventoryItemId ItemId, MemberId MemberId, int CommunityId = 1);

public class LoanToolCommandHandler(
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IRepository<MemberAgg, MemberId> memberRepository,
    IRepository<LoanAgg, LoanId> loanRepository,
    DomainEventOrchestrator orchestrator)
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

        var loanResult = LoanAgg.Create(command.MemberId, command.ItemId, command.CommunityId);
        if (loanResult.IsFailure)
            return loanResult.Error;

        var loan = loanResult.Value;
        await loanRepository.AddAsync(loan, cancellationToken);

        var loanItemResult = item.Loan(command.MemberId);
        if (loanItemResult.IsFailure)
            return loanItemResult.Error;

        inventoryRepository.Update(item);
        await orchestrator.SaveEntitiesAsync([loan, item], cancellationToken);

        return Result.Success();
    }
}
