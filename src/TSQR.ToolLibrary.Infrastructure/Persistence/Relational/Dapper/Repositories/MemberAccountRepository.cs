using TSQR.ToolLibrary.Application.Member.Accounts;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Repositories;

public sealed class MemberAccountRepository(ISqlUnitOfWork uow) : IMemberAccountRepository
{
    public async Task<MemberId?> GetBySubjectAsync(
        Guid authSubject,
        CancellationToken ct = default)
    {
        var memberId = await uow.Connection.ExecuteScalarAsync<int?>(
            "SELECT MemberId FROM MemberAccounts WHERE AuthSubject = @AuthSubject",
            new { AuthSubject = authSubject });

        return memberId.HasValue ? new MemberId(memberId.Value) : null;
    }

    public Task LinkAsync(
        MemberId memberId,
        Guid authSubject,
        CancellationToken ct = default) =>
        uow.Connection.ExecuteAsync(
            @"INSERT INTO MemberAccounts (MemberId, AuthSubject)
              VALUES (@MemberId, @AuthSubject)
              ON CONFLICT (MemberId) DO UPDATE SET AuthSubject = EXCLUDED.AuthSubject",
            new { MemberId = memberId.Value, AuthSubject = authSubject });
}
