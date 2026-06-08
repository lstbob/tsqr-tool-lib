namespace TSQR.ToolLibrary.Common.Errors;

public sealed record ValidationError(string Code, string Message) : Error(Code, Message);
