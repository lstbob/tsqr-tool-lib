namespace TSQR.ToolLibrary.Domain.Aggregates.MemberAggregate;

public class Member : Entity<MemberId>, IAggregateRoot
{
    private Member(
        MemberId id,
        string firstName,
        string middleName,
        string lastName,
        int age,
        string address,
        string email,
        string phoneNumber,
        MemberStatus status,
        bool isVerified,
        MemberId? verifiedByAdminId,
        DateTime? verificationDate,
        MembershipRecord? record = null) : base(id)
    {
        FirstName = firstName.Validate(nameof(firstName));
        MiddleName = middleName;
        LastName = lastName.Validate(nameof(lastName));
        Age = age;
        Address = address.Validate(nameof(address));
        Email = email.Validate(nameof(email));
        PhoneNumber = phoneNumber.Validate(nameof(phoneNumber));
        Status = status.ValidateDefined(nameof(status)).ValidateNotDefault(nameof(status));
        IsVerified = isVerified;
        VerifiedByAdminId = verifiedByAdminId;
        VerificationDate = verificationDate;
        Record = record;
    }

    public string FirstName { get; }
    public string MiddleName { get; }
    public string LastName { get; }
    public int Age { get; }
    public string Address { get; }
    public string Email { get; }
    public string PhoneNumber { get; }
    public MemberStatus Status { get; private set; }
    public bool IsVerified { get; private set; }
    public MemberId? VerifiedByAdminId { get; private set; }
    public DateTime? VerificationDate { get; private set; }
    public MembershipRecord? Record { get; private set; }

    public static Member Create(
        string firstName,
        string middleName,
        string lastName,
        int age,
        string address,
        string email,
        string phoneNumber,
        MemberStatus status,
        MembershipRecord? record = null)
    {
        return new(
            new MemberId(default),
            firstName,
            middleName,
            lastName,
            age,
            address,
            email,
            phoneNumber,
            status,
            false,
            null,
            null,
            record);
    }

    public static Member Create(
        MemberId id,
        string firstName,
        string middleName,
        string lastName,
        int age,
        string address,
        string email,
        string phoneNumber,
        MemberStatus status,
        bool isVerified,
        MemberId? verifiedByAdminId,
        DateTime? verificationDate,
        MembershipRecord? record = null)
    {
        return new(
            id,
            firstName,
            middleName,
            lastName,
            age,
            address,
            email,
            phoneNumber,
            status,
            isVerified,
            verifiedByAdminId,
            verificationDate,
            record);
    }

    public void Verify(MemberId adminId)
    {
        if (IsVerified)
            throw new InvalidOperationException("Member is already verified.");

        ArgumentNullException.ThrowIfNull(adminId);
        IsVerified = true;
        VerifiedByAdminId = adminId;
        VerificationDate = DateTime.UtcNow;
        Status = MemberStatus.Active;
        AddDomainEvent(new MemberVerifiedEvent(Id, adminId, VerificationDate.Value));
    }

    public void Suspend()
    {
        if (Status == MemberStatus.Suspended)
            throw new InvalidOperationException("Member is already suspended.");

        Status = MemberStatus.Suspended;
    }

    public void Ban()
    {
        if (Status == MemberStatus.Banned)
            throw new InvalidOperationException("Member is already banned.");

        Status = MemberStatus.Banned;
    }

    public void Reinstate()
    {
        if (Status != MemberStatus.Suspended && Status != MemberStatus.Banned)
            throw new InvalidOperationException("Only suspended or banned members can be reinstated.");

        Status = MemberStatus.Active;
    }

    public bool IsEligibleToBorrow()
    {
        return IsVerified && Status == MemberStatus.Active;
    }
}
