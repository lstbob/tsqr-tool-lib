namespace TSQR.ToolLibrary.Common.Errors;

public abstract record Error(string Code, string Message)
{
    public static readonly Error None = new ValidationError(string.Empty, string.Empty);
}
