using LoanAgg = TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate.Loan;
using PolicyAgg = TSQR.ToolLibrary.Domain.Aggregates.PolicyAggregate.Policy;

namespace TSQR.ToolLibrary.Application.Loan.Commands;

public record RenewLoanCommand(LoanId LoanId);

public class RenewLoanCommandHandler(
    IRepository<LoanAgg, LoanId> loanRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IToolRepository toolRepository,
    IPolicyRepository policyRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<RenewLoanCommand, Result>
{
    public async Task<Result> ExecuteAsync(RenewLoanCommand command, CancellationToken cancellationToken)
    {
        var loan = await loanRepository.GetByIdAsync(command.LoanId, cancellationToken);
        if (loan is null)
            return new NotFoundError(nameof(command.LoanId), "Loan not found.");

        var item = await inventoryRepository.GetByIdAsync(loan.ItemId, cancellationToken);
        if (item is null)
            return new NotFoundError(nameof(loan.ItemId), "Inventory item not found.");

        var tool = await toolRepository.GetByIdAsync(item.ToolId, cancellationToken);
        if (tool is null)
            return new NotFoundError(nameof(item.ToolId), "Tool referenced by inventory item not found.");

        var policy = await policyRepository.GetByToolTypeAsync(
            tool.Type, locationId: null, cancellationToken);
        if (policy is null)
            return new DomainError(nameof(PolicyAgg), "No lending policy configured for this tool type.");

        var renewResult = loan.Renew(policy.MaxLoanDurationDays, policy.MaxRenewalCount);
        if (renewResult.IsFailure)
            return renewResult.Error;

        loanRepository.Update(loan);
        await orchestrator.SaveEntitiesAsync(loan, cancellationToken);

        return Result.Success();
    }
}
