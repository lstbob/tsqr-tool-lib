namespace TSQR.ToolLibrary.Application.Member.Accounts;

/// <summary>
/// Maps an external authentication subject (JWT <c>sub</c>) to a tool-library
/// member. The mapping is explicit so an email change cannot transfer access.
/// </summary>
public interface IMemberAccountRepository
{
    Task<MemberId?> GetBySubjectAsync(Guid authSubject, CancellationToken ct = default);

    /// <summary>
    /// Resolves the community that owns the member linked to an auth subject.
    /// Returns <c>null</c> when the subject is not linked to a member.
    /// </summary>
    Task<int?> GetCommunityIdBySubjectAsync(Guid authSubject, CancellationToken ct = default);

    Task LinkAsync(MemberId memberId, Guid authSubject, CancellationToken ct = default);
}
