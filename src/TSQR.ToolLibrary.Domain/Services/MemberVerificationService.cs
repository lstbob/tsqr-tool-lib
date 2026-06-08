namespace TSQR.ToolLibrary.Domain.Services;

public class MemberVerificationService
{
    public Result<bool> IsEligibleToBorrow(Member member)
    {
        if (member is null)
            return new ValidationError(nameof(member), "Member is required.");

        return member.IsVerified && member.Status == MemberStatus.Active;
    }

    public Result<bool> CanVerifyMember(Member admin, Member targetMember)
    {
        if (admin is null)
            return new ValidationError(nameof(admin), "Admin is required.");
        if (targetMember is null)
            return new ValidationError(nameof(targetMember), "Target member is required.");

        if (admin.Record?.MembershipType != MembershipType.Admin &&
            admin.Record?.MembershipType != MembershipType.LocationCoordinator)
            return false;

        if (targetMember.IsVerified)
            return false;

        return true;
    }
}
