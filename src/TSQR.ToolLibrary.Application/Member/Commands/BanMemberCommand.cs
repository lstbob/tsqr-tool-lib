using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record BanMemberCommand(MemberId MemberId);

public class BanMemberCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<BanMemberCommand, Result>
{
    public async Task<Result> ExecuteAsync(BanMemberCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        var banResult = member.Ban();
        if (banResult.IsFailure)
            return banResult.Error;

        await memberRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(member.DomainEvents, cancellationToken);
        member.ClearDomainEvents();

        return Result.Success();
    }
}
