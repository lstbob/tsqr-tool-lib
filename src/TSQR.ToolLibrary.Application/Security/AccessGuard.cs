namespace TSQR.ToolLibrary.Application.Security;

/// <summary>
/// Shared caller/owner checks for commands that mutate member-owned data.
/// Staff callers may act across members when their capability policy allows it.
/// </summary>
public static class AccessGuard
{
    public static Result EnsureOwns(
        MemberId ownerId,
        MemberId? actorId,
        bool canManageAny,
        string resource)
    {
        if (canManageAny)
            return Result.Success();

        if (actorId is null || actorId != ownerId)
            return new ForbiddenError(
                "Forbidden",
                $"You are not allowed to modify this {resource}.");

        return Result.Success();
    }

    public static Result EnsureActingFor(
        MemberId targetId,
        MemberId? actorId,
        bool canManageAny,
        string action)
    {
        if (canManageAny)
            return Result.Success();

        if (actorId is null || actorId != targetId)
            return new ForbiddenError(
                "Forbidden",
                $"You are not allowed to {action} on behalf of another member.");

        return Result.Success();
    }
}
