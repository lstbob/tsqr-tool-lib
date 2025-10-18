namespace TSQR.ToolLibrary.Common.Extensions;

/// <summary>
/// Extension methods for integer validation.
/// </summary>
public static class IntegerExtensions
{
    /// <summary>
    /// Validates that the integer is positive (greater than zero).
    /// </summary>
    /// <param name="value">The integer to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the integer is not positive.</exception>
    /// <returns>The validated integer.</returns>
    public static int ValidatePositive(this int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be positive.");
        }

        return value;
    } 
}

