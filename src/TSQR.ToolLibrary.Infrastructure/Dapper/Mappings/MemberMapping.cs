namespace TSQR.ToolLibrary.Infrastructure.Dapper.Mappings;

internal sealed record MemberRow(
    MemberId Id,
    string FirstName,
    string MiddleName,
    string LastName,
    int Age,
    string Address,
    string Email,
    string PhoneNumber,
    MemberStatus Status,
    bool IsVerified,
    MemberId? VerifiedByAdminId,
    DateTime? VerificationDate,
    MembershipType? MembershipType,
    DateTime? StartDate,
    DateTime? EndDate);

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
    DateTime? EndDate);

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
    DateTime? EndDate);

public sealed class MemberMapping : IEntityMapping<Member>
{
    public string TableName => "Members";

    public string InsertSql =>
        @"INSERT INTO Members (FirstName, MiddleName, LastName, Age, Address, Email, PhoneNumber,
                Status, IsVerified, VerifiedByAdminId, VerificationDate,
                MembershipType, StartDate, EndDate)
          VALUES (@FirstName, @MiddleName, @LastName, @Age, @Address, @Email, @PhoneNumber,
                @Status, @IsVerified, @VerifiedByAdminId, @VerificationDate,
                @MembershipType, @StartDate, @EndDate);
          RETURNING Id";

    public string UpdateSql =>
        @"UPDATE Members
          SET FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
              Age = @Age, Address = @Address, Email = @Email, PhoneNumber = @PhoneNumber,
              Status = @Status, IsVerified = @IsVerified,
              VerifiedByAdminId = @VerifiedByAdminId, VerificationDate = @VerificationDate,
              MembershipType = @MembershipType, StartDate = @StartDate, EndDate = @EndDate
          WHERE Id = @Id";

    public string DeleteSql => "DELETE FROM Members WHERE Id = @Id";

    public async Task<Member?> GetByIdAsync(IDatabaseConnection db, object id)
    {
        var row = await db.QuerySingleOrDefaultAsync<MemberRow>(
            @"SELECT Id, FirstName, MiddleName, LastName, Age, Address, Email, PhoneNumber,
                     Status, IsVerified, VerifiedByAdminId, VerificationDate,
                     MembershipType, StartDate, EndDate
              FROM Members WHERE Id = @Id", new { Id = id });

        if (row is null) return null;

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
            record);
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
        entity.Record?.EndDate);

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
        entity.Record?.EndDate);
}
