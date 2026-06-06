namespace TSQR.ToolLibrary.Infrastructure.Dapper.Repositories;

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

public class MemberRepository : IRepository<Member, MemberId>
{
    private readonly DapperUnitOfWork _uow;

    public MemberRepository(DapperUnitOfWork uow) => _uow = uow;

    public IUnitOfWork UnitOfWork => _uow;

    public async Task<Member?> GetByIdAsync(MemberId id, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var row = await _uow.Connection.QuerySingleOrDefaultAsync<MemberRow>(
            @"SELECT Id, FirstName, MiddleName, LastName, Age, Address, Email, PhoneNumber,
                     Status, IsVerified, VerifiedByAdminId, VerificationDate,
                     MembershipType, StartDate, EndDate
              FROM Members WHERE Id = @Id",
            new { Id = id.Value },
            transaction: _uow.Transaction);

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

    public async Task AddAsync(Member entity, CancellationToken cancellationToken = default)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        var id = await _uow.Connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Members (FirstName, MiddleName, LastName, Age, Address, Email, PhoneNumber,
                   Status, IsVerified, VerifiedByAdminId, VerificationDate,
                   MembershipType, StartDate, EndDate)
              VALUES (@FirstName, @MiddleName, @LastName, @Age, @Address, @Email, @PhoneNumber,
                   @Status, @IsVerified, @VerifiedByAdminId, @VerificationDate,
                   @MembershipType, @StartDate, @EndDate);
              SELECT CAST(SCOPE_IDENTITY() AS INT)",
            new
            {
                entity.FirstName,
                entity.MiddleName,
                entity.LastName,
                entity.Age,
                entity.Address,
                entity.Email,
                entity.PhoneNumber,
                Status = entity.Status,
                entity.IsVerified,
                VerifiedByAdminId = entity.VerifiedByAdminId?.Value,
                entity.VerificationDate,
                MembershipType = entity.Record?.MembershipType,
                StartDate = entity.Record?.StartDate,
                EndDate = entity.Record?.EndDate
            },
            transaction: _uow.Transaction);

        entity.SetAssignedId(new MemberId(id));
    }

    public void Update(Member entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            @"UPDATE Members
              SET FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
                  Age = @Age, Address = @Address, Email = @Email, PhoneNumber = @PhoneNumber,
                  Status = @Status, IsVerified = @IsVerified,
                  VerifiedByAdminId = @VerifiedByAdminId, VerificationDate = @VerificationDate,
                  MembershipType = @MembershipType, StartDate = @StartDate, EndDate = @EndDate
              WHERE Id = @Id",
            new
            {
                Id = entity.Id.Value,
                entity.FirstName,
                entity.MiddleName,
                entity.LastName,
                entity.Age,
                entity.Address,
                entity.Email,
                entity.PhoneNumber,
                Status = entity.Status,
                entity.IsVerified,
                VerifiedByAdminId = entity.VerifiedByAdminId?.Value,
                entity.VerificationDate,
                MembershipType = entity.Record?.MembershipType,
                StartDate = entity.Record?.StartDate,
                EndDate = entity.Record?.EndDate
            },
            transaction: _uow.Transaction);
    }

    public void Delete(Member entity)
    {
        _uow.EnsureOpen();
        _uow.BeginTransaction();

        _uow.Connection.Execute(
            "DELETE FROM Members WHERE Id = @Id",
            new { Id = entity.Id.Value },
            transaction: _uow.Transaction);
    }
}
