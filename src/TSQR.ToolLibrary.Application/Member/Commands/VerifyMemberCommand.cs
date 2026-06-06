using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record VerifyMemberCommand(MemberId MemberId, MemberId VerifiedByAdminId) : IRequest<Result>;

public class VerifyMemberCommandHandler(IRepository<MemberAgg.Member, MemberId> memberRepository)
    : IRequestHandler<VerifyMemberCommand, Result>
{
    public async Task<Result> Handle(VerifyMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(request.MemberId), "Member not found.");

        var verifyResult = member.Verify(request.VerifiedByAdminId);
        if (verifyResult.IsFailure)
            return verifyResult.Error;

        await memberRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
