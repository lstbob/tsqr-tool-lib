namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

/// <summary>
/// Represents a member in the tool library system.
/// </summary>
public class Member : Entity<MemberId>
{
    /// <summary>
    /// Initializes a new instance of <see cref="Member"/>
    /// </summary>
    private Member(
            MemberId id,
            string firstName,
            string middleName,
            string lastName,
            DateTime dob,
            string address,
            MemberStatus status,
            MembershipRecord? membershipRecord = null
            ) : base(id)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        DoB = dob;
        Status = status;
        Address = address;
        MembershipRecord = membershipRecord;
    }

    public string FirstName {get; }
    public string MiddleName {get; }
    public string LastName{get; }
    public DateTime DoB {get; }
    public string Address {get; } 
    public MemberStatus Status {get;private set; }
    public MembershipRecord?  MembershipRecord {get; private set; }

    /// <summary>
    /// Responsible for creating a transient instance of the <see cref="Member"/> class.
    /// </summary>
    /// <param name="firstName">First name of the member.</param>
    /// <param name="middleName">Middle name of the member.</param>
    /// <param name="lastName">Last name of the member</param>
    /// <param name="dob">Date of birth of the member</param>
    /// <param name="address">Address of the member</param>
    /// <param name="status">Membership status of the member</param>
    /// <param name="membershipRecord">Membership record of the member</param>
    /// <returns>Member instance.</returns>
    public Member Create(
            string firstName,
            string middleName,
            string lastName,
            DateTime dob,
            string address,
            MemberStatus status,
            MembershipRecord? membershipRecord = null)
    {
        return new Member (
                new MemberId(default),
                firstName,
                middleName,
                lastName,
                dob,
                address,
                status,
                membershipRecord);
    }

    /// <summary>
    /// Responsible for rehydrating existing instance of the <see cref="Member"/> class.
    /// </summary>
    /// <param name="firstName">First name of the member.</param>
    /// <param name="firstName">First name of the member.</param>
    /// <param name="middleName">Middle name of the member.</param>
    /// <param name="lastName">Last name of the member</param>
    /// <param name="dob">Date of birth of the member</param>
    /// <param name="address">Address of the member</param>
    /// <param name="status">Membership status of the member</param>
    /// <param name="membershipRecord">Membership record of the member</param>
    /// <returns>Member instance.</returns>
    public Member Create(
            MemberId id,
            string firstName,
            string middleName,
            string lastName,
            DateTime dob,
            string address,
            MemberStatus status,
            MembershipRecord? membershipRecord = null)
    {
        return new(id, firstName, middleName, lastName, dob, address, status, membershipRecord);
    }
}
