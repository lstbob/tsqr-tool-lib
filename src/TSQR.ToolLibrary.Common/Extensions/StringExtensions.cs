namespace TSQR.ToolLibrary.Common.Extensions;

/// <summary>
/// Extension methods for string validation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Validates that the string is neither null nor whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown if the string is null or whitespace.</exception
    /// ><returns>The validated string.</returns>
    public static string Validate(this string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }

        return value!;
    }
}

