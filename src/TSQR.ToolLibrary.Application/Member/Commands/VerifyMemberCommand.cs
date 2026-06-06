using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record VerifyMemberCommand(MemberId MemberId, MemberId VerifiedByAdminId) : IRequest;

public class VerifyMemberCommandHandler(IRepository<MemberAgg.Member, MemberId> memberRepository)
    : IRequestHandler<VerifyMemberCommand>
{
    public async Task Handle(VerifyMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken)
            ?? throw new InvalidOperationException("Member not found.");

        member.Verify(request.VerifiedByAdminId);
        await memberRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
