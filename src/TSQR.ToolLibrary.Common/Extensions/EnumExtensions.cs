using TSQR.ToolLibrary.Common.Errors;
using TSQR.ToolLibrary.Common.Results;

namespace TSQR.ToolLibrary.Common.Extensions;

public static class EnumExtensions
{
    public static Result<TEnum> ValidateDefined<TEnum>(this TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            return new ValidationError(paramName, $"Value '{value}' is not defined in enum '{typeof(TEnum).Name}'.");

        return value;
    }

    public static Result<TEnum> ValidateNotDefault<TEnum>(this TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (value.Equals(default(TEnum)))
            return new ValidationError(paramName, $"Enum value cannot be the default value '{default(TEnum)}'.");

        return value;
    }
}
