namespace TSQR.ToolLibrary.Domain.Services;

public class MemberVerificationService
{
    public bool IsEligibleToBorrow(Member member)
    {
        ArgumentNullException.ThrowIfNull(member);
        return member.IsVerified && member.Status == MemberStatus.Active;
    }

    public bool CanVerifyMember(Member admin, Member targetMember)
    {
        ArgumentNullException.ThrowIfNull(admin);
        ArgumentNullException.ThrowIfNull(targetMember);

        if (admin.Record?.MembershipType != MembershipType.Admin &&
            admin.Record?.MembershipType != MembershipType.LocationCoordinator)
            return false;

        if (targetMember.IsVerified)
            return false;

        return true;
    }
}
