using System.Security.Claims;
using TSQR.Common.Authorization;

namespace TSQR.ToolLibrary.WebApi.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public string? Subject => Principal?.FindFirstValue("sub")
        ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => Principal?.FindFirstValue("email")
        ?? Principal?.FindFirstValue(ClaimTypes.Email);

    public bool HasPermission(string permission)
    {
        if (Principal?.Identity?.IsAuthenticated != true)
            return false;

        foreach (var claim in Principal.FindAll("permission").Concat(Principal.FindAll("permissions")))
        {
            foreach (var value in claim.Value.Split(
                [' ', ',', ';'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (string.Equals(value, permission, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        var roles = Principal.FindAll(ClaimTypes.Role).Concat(Principal.FindAll("role"));
        return roles.Any(role => RolePermissions.RoleHasPermission(role.Value, permission));
    }
}
