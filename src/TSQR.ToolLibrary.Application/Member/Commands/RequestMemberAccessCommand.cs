using MemberAgg = TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate.Member;

namespace TSQR.ToolLibrary.Application.Member.Commands;

public record RequestMemberAccessCommand(
    MemberId MemberId,
    IdentificationType IdentificationType,
    string IdentificationReference);

public class RequestMemberAccessCommandHandler(
    IRepository<MemberAgg, MemberId> memberRepository,
    IDomainEventDispatcher eventDispatcher)
    : IInteractor<RequestMemberAccessCommand, Result>
{
    public async Task<Result> ExecuteAsync(RequestMemberAccessCommand command, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(command.MemberId, cancellationToken);
        if (member is null)
            return new NotFoundError(nameof(command.MemberId), "Member not found.");

        var identificationResult = Identification.Create(
            command.IdentificationType,
            command.IdentificationReference);

        if (identificationResult.IsFailure)
            return identificationResult.Error;

        var requestResult = member.RequestAccess(identificationResult.Value);
        if (requestResult.IsFailure)
            return requestResult.Error;

        await memberRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        await eventDispatcher.DispatchAsync(member.DomainEvents, cancellationToken);
        member.ClearDomainEvents();

        return Result.Success();
    }
}
