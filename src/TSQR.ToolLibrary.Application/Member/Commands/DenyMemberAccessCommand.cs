using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record DenyMemberAccessCommand(MemberId MemberId);

public class DenyMemberAccessCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<DenyMemberAccessCommand, Result>
{
    public async Task<Result> ExecuteAsync(DenyMemberAccessCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        var denyResult = member.DenyAccess();
        if (denyResult.IsFailure)
            return denyResult.Error;

        await memberRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(member.DomainEvents, cancellationToken);
        member.ClearDomainEvents();

        return Result.Success();
    }
}
