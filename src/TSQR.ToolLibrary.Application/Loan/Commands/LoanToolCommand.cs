using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;
using LoanAgg = TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate.Loan;
using ToolAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Tool;
using PolicyAgg = TSQR.ToolLibrary.Domain.Aggregates.ToolAggregate.Policy;

namespace TSQR.ToolLibrary.Application.Loan.Commands;

public record LoanToolCommand(InventoryItemId ItemId, MemberId MemberId, int CommunityId = 1);

public class LoanToolCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    IRepository<LoanAgg, LoanId> loanRepository,
    IRepository<InventoryItem, InventoryItemId> inventoryRepository,
    IToolRepository toolRepository,
    IPolicyRepository policyRepository,
    IReservationRepository reservationRepository,
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

        var reservations = await reservationRepository.GetByItemIdAsync(command.ItemId, cancellationToken);
        var activeReservation = reservations.FirstOrDefault(r => r.Status is ReservationStatus.Confirmed or ReservationStatus.Active);
        if (activeReservation is not null && activeReservation.MemberId != command.MemberId)
            return new DomainError(nameof(command.ItemId), "Tool is reserved by another member.");

        // Load the Tool so we can resolve the active Policy by ToolType. The
        // (ToolType, null) lookup returns the global policy for the tool type;
        // per-location policies are an additive follow-up. If no policy is
        // configured, the handler rejects the loan: lending without a policy
        // would silently fall back to defaults and re-introduce the orphaned-
        // policy pattern this command was refactored to close (issue #35).
        var tool = await toolRepository.GetByIdAsync(item.ToolId, cancellationToken);
        if (tool is null)
            return new NotFoundError(nameof(item.ToolId), "Tool referenced by inventory item not found.");

        var policy = await policyRepository.GetByToolTypeAsync(tool.Type, locationId: null, cancellationToken);
        if (policy is null)
            return new DomainError(nameof(PolicyAgg), "No lending policy configured for this tool type.");

        // Policy-driven factory: snapshots MaxLoanDurationDays as the new
        // DueDate, and LateFeePerDay on the Loan so the rate captured at
        // checkout is the one applied at return time. Policy changes after
        // checkout cannot retroactively affect in-flight loans.
        var loanResult = LoanAgg.Create(
            command.MemberId,
            command.ItemId,
            policy.MaxLoanDurationDays,
            policy.LateFeePerDay,
            command.CommunityId);
        if (loanResult.IsFailure)
            return loanResult.Error;

        var loan = loanResult.Value;
        await loanRepository.AddAsync(loan, cancellationToken);
        await orchestrator.SaveEntitiesAsync(loan, cancellationToken);

        return Result.Success();
    }
}