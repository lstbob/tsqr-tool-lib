using TSQR.ToolLibrary.Common.Errors;
using TSQR.ToolLibrary.Common.Results;

namespace TSQR.ToolLibrary.Common.Extensions;

public static class CollectionExtensions
{
    public static Result<List<T>> ValidateNotEmpty<T>(this List<T> list, string paramName)
    {
        if (list == null || list.Count == 0)
            return new ValidationError(paramName, $"{paramName} cannot be null or empty.");

        return list;
    }
}
