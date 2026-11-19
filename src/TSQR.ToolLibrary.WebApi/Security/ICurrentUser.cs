namespace TSQR.ToolLibrary.WebApi.Security;

public interface ICurrentUser
{
    string? Subject { get; }

    string? Email { get; }

    bool HasPermission(string permission);
}
