namespace TSQR.ToolLibrary.Common.Errors;

public sealed record DomainError(string Code, string Message) : Error(Code, Message);
