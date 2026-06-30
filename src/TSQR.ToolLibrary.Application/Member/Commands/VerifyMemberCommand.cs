using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record VerifyMemberCommand(MemberId MemberId, MemberId VerifiedByAdminId);

public class VerifyMemberCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<VerifyMemberCommand, Result>
{
    public async Task<Result> ExecuteAsync(VerifyMemberCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        var verifyResult = member.Verify(command.VerifiedByAdminId);
        if (verifyResult.IsFailure)
            return verifyResult.Error;

        memberRepository.Update(member);
        await orchestrator.SaveEntitiesAsync(member, cancellationToken);

        return Result.Success();
    }
}
