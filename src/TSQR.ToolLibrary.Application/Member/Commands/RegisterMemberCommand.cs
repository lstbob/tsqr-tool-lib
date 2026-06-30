using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record RegisterMemberCommand(
    string FirstName,
    string MiddleName,
    string LastName,
    int Age,
    string Address,
    string Email,
    string PhoneNumber,
    MemberStatus Status,
    MembershipRecord? Record = null);

public class RegisterMemberCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    DomainEventOrchestrator orchestrator)
    : IInteractor<RegisterMemberCommand, Result<MemberId>>
{
    public async Task<Result<MemberId>> ExecuteAsync(RegisterMemberCommand command, CancellationToken cancellationToken)
    {
        var memberResult = MemberAgg.Create(
            command.FirstName,
            command.MiddleName,
            command.LastName,
            command.Age,
            command.Address,
            command.Email,
            command.PhoneNumber,
            command.Status,
            command.Record);

        if (memberResult.IsFailure)
            return memberResult.Error;

        var member = memberResult.Value;
        await memberRepository.AddAsync(member, cancellationToken);
        await orchestrator.SaveEntitiesAsync(member, cancellationToken);

        return member.Id;
    }
}
