namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

/// <summary>
/// Represents a member in the tool library system.
/// </summary>
public class Member : Entity<MemberId>
{
    private Member(
            MemberId id,
            string firstName,
            string middleName,
            string lastName,
            int age,
            string address,
            MemberStatus status,
            MembershipRecord record = null
            ) : base(id)
    {
        
    }
}

