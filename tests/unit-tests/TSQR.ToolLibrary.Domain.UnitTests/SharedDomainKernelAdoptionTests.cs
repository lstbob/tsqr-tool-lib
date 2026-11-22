using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

/// <summary>
/// Proves that tool-lib aggregates pick up the shared domain-kernel semantics
/// instead of the removed local base classes.
/// </summary>
public class SharedDomainKernelAdoptionTests
{
    [Fact]
    public void NewMemberWithZeroId_IsTransient()
    {
        var member = MakeMember(new MemberId(0));

        Assert.True(member.IsTransient());
    }

    [Fact]
    public void MembersWithTheSamePersistedId_AreEqual()
    {
        var first = MakeMember(new MemberId(7));
        var second = MakeMember(new MemberId(7));
        var different = MakeMember(new MemberId(8));

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.False(first.Equals(different));
    }

    [Fact]
    public void TwoTransientMembers_AreNotEqual()
    {
        var first = MakeMember(new MemberId(0));
        var second = MakeMember(new MemberId(0));

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void PersistenceHooks_AssignIdAndRowVersion()
    {
        var member = MakeMember(new MemberId(0));

        member.SetAssignedId(new MemberId(42));
        member.SetRowVersion(3);

        Assert.Equal(42, member.Id.Value);
        Assert.Equal(3, member.RowVersion);
        Assert.False(member.IsTransient());
    }

    private static Member MakeMember(MemberId id) =>
        Member.Create(
            id,
            "Alice",
            "",
            "Smith",
            30,
            "1 Main St",
            "alice@example.com",
            "555-0100",
            MemberStatus.Active,
            isVerified: true,
            verifiedByAdminId: null,
            verificationDate: null,
            communityId: 1);
}
