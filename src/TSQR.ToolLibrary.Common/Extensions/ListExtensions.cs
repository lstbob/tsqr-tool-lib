namespace TSQR.ToolLibrary.Common.Extensions;

/// <summary>
/// Extension methods for List.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Validates that the list is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to validate.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The original list if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if the list is null or empty.</exception>
   public static List<T> ValidateNotEmpty<T>(this List<T> list, string paramName)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("Collection cannot be null or empty.", paramName);
        }

        return list;
    }
}

