using TSQR.ToolLibrary.Common.Errors;
using TSQR.ToolLibrary.Common.Results;

namespace TSQR.ToolLibrary.Common.Extensions;

public static class DateTimeExtensions
{
    public static Result<DateTime> ValidateNotInPast(this DateTime value, string paramName)
    {
        if (value < DateTime.UtcNow)
            return new ValidationError(paramName, $"{paramName} cannot be in the past.");

        return value;
    }

    public static Result<DateTime> Validate(this DateTime value, string paramName)
    {
        if (value == default)
            return new ValidationError(paramName, $"{paramName} cannot be the default value.");

        return value;
    }

    public static Result<DateTime> ValidateNotInFuture(this DateTime value, string paramName)
    {
        if (value > DateTime.UtcNow)
            return new ValidationError(paramName, $"{paramName} cannot be in the future.");

        return value;
    }
}
