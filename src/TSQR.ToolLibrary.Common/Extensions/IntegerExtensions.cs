using TSQR.ToolLibrary.Common.Errors;
using TSQR.ToolLibrary.Common.Results;

namespace TSQR.ToolLibrary.Common.Extensions;

public static class IntegerExtensions
{
    public static Result<int> ValidatePositive(this int value, string paramName)
    {
        if (value <= 0)
            return new ValidationError(paramName, $"{paramName} must be positive.");

        return value;
    }
}
