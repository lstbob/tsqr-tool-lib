namespace TSQR.ToolLibrary.Common.Extensions;

/// <summary>
/// Extension methods for DateTime validation.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Validates that the DateTime is not in the past.
    /// </summary>
    /// <param name="value">The DateTime to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the DateTime is in the past.</exception>
    /// <returns>The validated DateTime.</returns>
    public static DateTime ValidateNotInPast(this DateTime value, string paramName)
    {
        if (value < DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(paramName, "DateTime cannot be in the past.");
        }

        return value;
    }

    /// <summary>
    /// Validates that the DateTime is not the default value.
    /// </summary>
    /// <param name="value">The DateTime to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown if the DateTime is the default value.</exception>
    /// <returns>The validated DateTime.</returns>
    public static DateTime Validate(this DateTime value, string paramName)
    {
        if (value == default)
        {
            throw new ArgumentException("DateTime cannot be the default value.", paramName);
        }

        return value;
    }

    /// <summary>
    /// Validates that the DateTime is not in the future.
    /// </summary>
    /// <param name="value">The DateTime to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the DateTime is in the future.</exception>
    /// <returns>The validated DateTime.</returns>
    public static DateTime ValidateNotInFuture(this DateTime value, string paramName)
    {
        if (value > DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(paramName, "DateTime cannot be in the future.");
        }

        return value;
    } 
}

