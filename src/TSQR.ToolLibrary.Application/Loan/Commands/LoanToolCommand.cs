using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;
using LoanAgg = TSQR.ToolLibrary.Domain.Aggregates.LoanAggregate.Loan;

namespace TSQR.ToolLibrary.Application.Loan.Commands;

public record LoanToolCommand(InventoryItemId ItemId, MemberId MemberId, int CommunityId = 1);

public class LoanToolCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    IRepository<LoanAgg, LoanId> loanRepository,
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

        var reservations = await reservationRepository.GetByItemIdAsync(command.ItemId, cancellationToken);
        var activeReservation = reservations.FirstOrDefault(r => r.Status is ReservationStatus.Confirmed or ReservationStatus.Active);
        if (activeReservation is not null && activeReservation.MemberId != command.MemberId)
            return new DomainError(nameof(command.ItemId), "Tool is reserved by another member.");

        var loanResult = LoanAgg.Create(command.MemberId, command.ItemId, command.CommunityId);
        if (loanResult.IsFailure)
            return loanResult.Error;

        var loan = loanResult.Value;
        await loanRepository.AddAsync(loan, cancellationToken);
        await orchestrator.SaveEntitiesAsync(loan, cancellationToken);

        return Result.Success();
    }
}
