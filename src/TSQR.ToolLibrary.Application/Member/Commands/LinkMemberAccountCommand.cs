using TSQR.ToolLibrary.Application.Member.Accounts;
using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record LinkMemberAccountCommand(MemberId MemberId, Guid AuthSubject);

public class LinkMemberAccountCommandHandler(
    IRepository<MemberAgg, MemberId> members,
    IMemberAccountRepository accounts,
    IUnitOfWork uow)
    : IInteractor<LinkMemberAccountCommand, Result>
{
    public async Task<Result> ExecuteAsync(
        LinkMemberAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.AuthSubject == Guid.Empty)
            return new ValidationError(nameof(command.AuthSubject), "Auth subject is required.");

        var member = await members.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        var existing = await accounts.GetBySubjectAsync(command.AuthSubject, cancellationToken);
        if (existing is not null && existing != command.MemberId)
            return new ValidationError(
                nameof(command.AuthSubject),
                "Auth subject is already linked to another member.");

        await accounts.LinkAsync(command.MemberId, command.AuthSubject, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
