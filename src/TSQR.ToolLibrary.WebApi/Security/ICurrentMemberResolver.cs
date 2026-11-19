namespace TSQR.ToolLibrary.WebApi.Security;

public interface ICurrentMemberResolver
{
    Task<MemberId?> GetMemberIdAsync(CancellationToken ct = default);
}
