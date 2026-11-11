using TSQR.Common.Errors;
using TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class MemberBorrowingEligibilityTests
{
    private static readonly MemberId AdminId = new(99);

    [Fact]
    public void EnsureEligibleToBorrow_VerifiedActiveMember_ReturnsSuccess()
    {
        var member = MakeMember();

        var result = member.EnsureEligibleToBorrow(
            outstandingFineBalance: 0m,
            activeOverdueLoanCount: 0);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void EnsureEligibleToBorrow_AtFineLimit_ReturnsSuccess()
    {
        var member = MakeMember();

        var result = member.EnsureEligibleToBorrow(
            Member.MaxOutstandingFineBalance,
            activeOverdueLoanCount: 0);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void EnsureEligibleToBorrow_OverFineLimit_ReturnsDomainError()
    {
        var member = MakeMember();

        var result = member.EnsureEligibleToBorrow(
            Member.MaxOutstandingFineBalance + 0.01m,
            activeOverdueLoanCount: 0);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("outstandingFineBalance", error.Code);
    }

    [Fact]
    public void EnsureEligibleToBorrow_WithOverdueLoan_ReturnsDomainError()
    {
        var member = MakeMember();

        var result = member.EnsureEligibleToBorrow(
            outstandingFineBalance: 0m,
            activeOverdueLoanCount: 1);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("activeOverdueLoanCount", error.Code);
    }

    [Fact]
    public void EnsureEligibleToBorrow_SuspendedMember_ReturnsDomainError()
    {
        var member = MakeMember(MemberStatus.Suspended);

        var result = member.EnsureEligibleToBorrow(
            outstandingFineBalance: 0m,
            activeOverdueLoanCount: 0);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("Status", error.Code);
    }

    [Fact]
    public void EnsureEligibleToBorrow_UnverifiedMember_ReturnsDomainError()
    {
        var member = MakeMember(isVerified: false);

        var result = member.EnsureEligibleToBorrow(
            outstandingFineBalance: 0m,
            activeOverdueLoanCount: 0);

        Assert.True(result.IsFailure);
        var error = Assert.IsType<DomainError>(result.Error);
        Assert.Equal("Status", error.Code);
    }

    private static Member MakeMember(
        MemberStatus status = MemberStatus.Active,
        bool isVerified = true) =>
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
            isVerified,
            isVerified ? AdminId : null,
            isVerified ? DateTime.UtcNow : null,
            communityId: 1);
}
