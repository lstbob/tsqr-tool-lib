using TSQR.Common.Errors;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class MemberStateTransitionTests
{
    private static readonly MemberId AdminId = new(99);

    [Fact]
    public void Reinstate_SuspendedMember_ActivatesMember()
    {
        var member = MakeMember(MemberStatus.Suspended);

        var result = member.Reinstate();

        Assert.True(result.IsSuccess);
        Assert.Equal(MemberStatus.Active, member.Status);
    }

    [Fact]
    public void Reinstate_BannedMember_IsRejectedAndLeavesStatusBanned()
    {
        var member = MakeMember(MemberStatus.Banned);

        var result = member.Reinstate();

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("Status", error.Code);
        Assert.Equal(MemberStatus.Banned, member.Status);
    }

    [Fact]
    public void Reinstate_ActiveMember_IsRejected()
    {
        var member = MakeMember(MemberStatus.Active);

        var result = member.Reinstate();

        Assert.True(result.IsFailure);
        Assert.IsType<DomainError>(result.Error);
        Assert.Equal(MemberStatus.Active, member.Status);
    }

    private static Member MakeMember(MemberStatus status) =>
        Member.Create(
            new MemberId(1),
            "Alice",
            "",
            "Smith",
            30,
            "1 Main St",
            "alice@example.com",
            "555-0100",
            status,
            isVerified: true,
            AdminId,
            DateTime.UtcNow,
            communityId: 1);
}
