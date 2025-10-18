namespace TSQR.ToolLibrary.Common.Extensions;

/// <summary>
/// Extension methods for enums.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Validates that the enum value is defined in the enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown if the enum value is not defined.</exception>
    /// <returns>The validated enum value.</returns>
    public static TEnum ValidateDefined<TEnum>(this TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentException($"Value '{value}' is not defined in enum '{typeof(TEnum).Name}'.", paramName);
        }

        return value;
    }

    /// <summary>
    /// Validates that the enum value is not the default value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown if the enum value is the default value.</exception>
    /// <returns>The validated enum value.</returns>
    public static TEnum ValidateNotDefault<TEnum>(this TEnum value, string paramName) where TEnum : struct, Enum
    {
        if (value.Equals(default(TEnum)))
        {
            throw new ArgumentException($"Enum value cannot be the default value '{default(TEnum)}'.", paramName);
        }

        return value;
    }
}

