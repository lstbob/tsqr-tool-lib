namespace TSQR.ToolLibrary.Application.Errors;

/// <summary>
/// The authenticated caller is not allowed to perform the requested action.
/// Mapped to HTTP 403 by the WebApi controllers.
/// </summary>
public sealed record ForbiddenError(string Code, string Message) : Error(Code, Message);
