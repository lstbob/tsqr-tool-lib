using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record SuspendMemberCommand(MemberId MemberId);

public class SuspendMemberCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<SuspendMemberCommand, Result>
{
    public async Task<Result> ExecuteAsync(SuspendMemberCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        var suspendResult = member.Suspend();
        if (suspendResult.IsFailure)
            return suspendResult.Error;

        await memberRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(member.DomainEvents, cancellationToken);
        member.ClearDomainEvents();

        return Result.Success();
    }
}
