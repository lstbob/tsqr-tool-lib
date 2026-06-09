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
        MembershipRecord? record = null,
        Identification? identification = null,
        AccessRequestStatus accessRequestStatus = AccessRequestStatus.NotSet) : base(id)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Age = age;
        Address = address;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = status;
        IsVerified = isVerified;
        VerifiedByAdminId = verifiedByAdminId;
        VerificationDate = verificationDate;
        Record = record;
        Identification = identification;
        AccessRequestStatus = accessRequestStatus;
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
    public Identification? Identification { get; private set; }
    public AccessRequestStatus AccessRequestStatus { get; private set; }

    public static Result<Member> Create(
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
        var firstNameResult = firstName.Validate(nameof(firstName));
        if (firstNameResult.IsFailure) return firstNameResult.Error;

        var lastNameResult = lastName.Validate(nameof(lastName));
        if (lastNameResult.IsFailure) return lastNameResult.Error;

        var addressResult = address.Validate(nameof(address));
        if (addressResult.IsFailure) return addressResult.Error;

        var emailResult = email.Validate(nameof(email));
        if (emailResult.IsFailure) return emailResult.Error;

        var phoneResult = phoneNumber.Validate(nameof(phoneNumber));
        if (phoneResult.IsFailure) return phoneResult.Error;

        var statusResult = status.ValidateDefined(nameof(status));
        if (statusResult.IsFailure) return statusResult.Error;

        var notDefaultResult = status.ValidateNotDefault(nameof(status));
        if (notDefaultResult.IsFailure) return notDefaultResult.Error;

        return new Member(
            new MemberId(default),
            firstNameResult.Value,
            middleName,
            lastNameResult.Value,
            age,
            addressResult.Value,
            emailResult.Value,
            phoneResult.Value,
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
        MembershipRecord? record = null,
        Identification? identification = null,
        AccessRequestStatus accessRequestStatus = AccessRequestStatus.NotSet)
    {
        return new Member(
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
            record,
            identification,
            accessRequestStatus);
    }

    public Result RequestAccess(Identification identification)
    {
        if (identification is null)
            return new ValidationError(nameof(identification), "Identification is required.");

        if (AccessRequestStatus == AccessRequestStatus.Pending)
            return new DomainError(nameof(AccessRequestStatus), "Access request is already pending.");

        if (AccessRequestStatus == AccessRequestStatus.Approved)
            return new DomainError(nameof(AccessRequestStatus), "Access has already been approved.");

        Identification = identification;
        AccessRequestStatus = AccessRequestStatus.Pending;
        return Result.Success();
    }

    public Result ApproveAccess(MemberId adminId)
    {
        if (AccessRequestStatus != AccessRequestStatus.Pending)
            return new DomainError(nameof(AccessRequestStatus), "No pending access request to approve.");

        if (adminId is null)
            return new ValidationError(nameof(adminId), "Admin ID is required.");

        AccessRequestStatus = AccessRequestStatus.Approved;
        return Verify(adminId);
    }

    public Result DenyAccess()
    {
        if (AccessRequestStatus != AccessRequestStatus.Pending)
            return new DomainError(nameof(AccessRequestStatus), "No pending access request to deny.");

        AccessRequestStatus = AccessRequestStatus.Denied;
        return Result.Success();
    }

    public Result Verify(MemberId adminId)
    {
        if (IsVerified)
            return new DomainError(nameof(IsVerified), "Member is already verified.");
        if (adminId is null)
            return new ValidationError(nameof(adminId), "Admin ID is required.");

        IsVerified = true;
        VerifiedByAdminId = adminId;
        VerificationDate = DateTime.UtcNow;
        Status = MemberStatus.Active;
        AddDomainEvent(new MemberVerifiedEvent(Id, adminId, VerificationDate.Value));
        return Result.Success();
    }

    public Result Suspend()
    {
        if (Status == MemberStatus.Suspended)
            return new DomainError(nameof(Status), "Member is already suspended.");

        Status = MemberStatus.Suspended;
        return Result.Success();
    }

    public Result Ban()
    {
        if (Status == MemberStatus.Banned)
            return new DomainError(nameof(Status), "Member is already banned.");

        Status = MemberStatus.Banned;
        return Result.Success();
    }

    public Result Reinstate()
    {
        if (Status != MemberStatus.Suspended && Status != MemberStatus.Banned)
            return new DomainError(nameof(Status), "Only suspended or banned members can be reinstated.");

        Status = MemberStatus.Active;
        return Result.Success();
    }

    public bool IsEligibleToBorrow()
    {
        return IsVerified && Status == MemberStatus.Active;
    }
}
