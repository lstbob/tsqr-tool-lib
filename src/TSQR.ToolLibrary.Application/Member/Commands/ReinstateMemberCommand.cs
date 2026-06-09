using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record ReinstateMemberCommand(MemberId MemberId);

public class ReinstateMemberCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<ReinstateMemberCommand, Result>
{
    public async Task<Result> ExecuteAsync(ReinstateMemberCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        var reinstateResult = member.Reinstate();
        if (reinstateResult.IsFailure)
            return reinstateResult.Error;

        await memberRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(member.DomainEvents, cancellationToken);
        member.ClearDomainEvents();

        return Result.Success();
    }
}
