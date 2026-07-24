using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Mappings;

internal sealed record MemberRow
{
    public MemberId Id { get; init; }
    public string FirstName { get; init; }
    public string MiddleName { get; init; }
    public string LastName { get; init; }
    public int Age { get; init; }
    public string Address { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
    public MemberStatus Status { get; init; }
    public bool IsVerified { get; init; }
    public MemberId? VerifiedByAdminId { get; init; }
    public DateTime? VerificationDate { get; init; }
    public MembershipType? MembershipType { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int CommunityId { get; init; }
}

internal sealed record MemberInsertDto(
    string FirstName,
    string MiddleName,
    string LastName,
    int Age,
    string Address,
    string Email,
    string PhoneNumber,
    MemberStatus Status,
    bool IsVerified,
    int? VerifiedByAdminId,
    DateTime? VerificationDate,
    MembershipType? MembershipType,
    DateTime? StartDate,
    DateTime? EndDate,
    int CommunityId);

internal sealed record MemberUpdateDto(
    int Id,
    string FirstName,
    string MiddleName,
    string LastName,
    int Age,
    string Address,
    string Email,
    string PhoneNumber,
    MemberStatus Status,
    bool IsVerified,
    int? VerifiedByAdminId,
    DateTime? VerificationDate,
    MembershipType? MembershipType,
    DateTime? StartDate,
    DateTime? EndDate,
    int CommunityId);

public sealed class MemberMapping : ISqlEntityMapping<Member>
{
    public string TableName => "Members";

    public string InsertSql =>
        @"INSERT INTO Members (FirstName, MiddleName, LastName, Age, Address, Email, PhoneNumber,
                Status, IsVerified, VerifiedByAdminId, VerificationDate,
                MembershipType, StartDate, EndDate, CommunityId)
          VALUES (@FirstName, @MiddleName, @LastName, @Age, @Address, @Email, @PhoneNumber,
                @Status, @IsVerified, @VerifiedByAdminId, @VerificationDate,
                @MembershipType, @StartDate, @EndDate, @CommunityId)
";

    public string UpdateSql =>
        @"UPDATE Members
          SET FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
              Age = @Age, Address = @Address, Email = @Email, PhoneNumber = @PhoneNumber,
              Status = @Status, IsVerified = @IsVerified,
              VerifiedByAdminId = @VerifiedByAdminId, VerificationDate = @VerificationDate,
              MembershipType = @MembershipType, StartDate = @StartDate, EndDate = @EndDate,
              CommunityId = @CommunityId
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM Members WHERE Id = @Id";

    public async Task<Member?> GetByIdAsync(ISqlConnection db, int id)
    {
        var row = await db.QuerySingleOrDefaultAsync<MemberRow>(
            @"SELECT Id, FirstName, MiddleName, LastName, Age, Address, Email, PhoneNumber,
                     Status, IsVerified, VerifiedByAdminId, VerificationDate,
                     MembershipType, StartDate, EndDate, CommunityId
              FROM Members WHERE Id = @Id", new { Id = id });

        if (row is null)
            return null;

        MembershipRecord? record = row.MembershipType.HasValue
            ? MembershipRecord.Create(row.StartDate!.Value, row.MembershipType.Value)
            : null;

        return Member.Create(
            row.Id,
            row.FirstName,
            row.MiddleName,
            row.LastName,
            row.Age,
            row.Address,
            row.Email,
            row.PhoneNumber,
            row.Status,
            row.IsVerified,
            row.VerifiedByAdminId,
            row.VerificationDate,
            record,
            communityId: row.CommunityId);
    }

    public object ToInsertParameters(Member entity) => new MemberInsertDto(
        entity.FirstName,
        entity.MiddleName,
        entity.LastName,
        entity.Age,
        entity.Address,
        entity.Email,
        entity.PhoneNumber,
        entity.Status,
        entity.IsVerified,
        entity.VerifiedByAdminId?.Value,
        entity.VerificationDate,
        entity.Record?.MembershipType,
        entity.Record?.StartDate,
        entity.Record?.EndDate,
        entity.CommunityId);

    public object ToUpdateParameters(Member entity) => new MemberUpdateDto(
        entity.Id.Value,
        entity.FirstName,
        entity.MiddleName,
        entity.LastName,
        entity.Age,
        entity.Address,
        entity.Email,
        entity.PhoneNumber,
        entity.Status,
        entity.IsVerified,
        entity.VerifiedByAdminId?.Value,
        entity.VerificationDate,
        entity.Record?.MembershipType,
        entity.Record?.StartDate,
        entity.Record?.EndDate,
        entity.CommunityId);
}
