namespace TSQR.ToolLibrary.Domain.Events;

public record MemberVerifiedEvent(MemberId MemberId, MemberId VerifiedByAdminId, DateTime VerifiedDate) : IDomainEvent;
