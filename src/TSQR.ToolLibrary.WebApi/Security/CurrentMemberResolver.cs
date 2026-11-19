using TSQR.ToolLibrary.Application.Member.Accounts;

namespace TSQR.ToolLibrary.WebApi.Security;

public sealed class CurrentMemberResolver(
    ICurrentUser currentUser,
    IMemberAccountRepository accounts) : ICurrentMemberResolver
{
    public async Task<MemberId?> GetMemberIdAsync(CancellationToken ct = default)
    {
        if (!Guid.TryParse(currentUser.Subject, out var authSubject))
            return null;

        return await accounts.GetBySubjectAsync(authSubject, ct);
    }
}
