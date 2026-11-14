using TSQR.Common.Exceptions;

namespace TSQR.ToolLibrary.Domain;

/// <summary>
/// Raised when an optimistic-concurrency token no longer matches the stored
/// row, meaning another transaction modified or deleted the aggregate after it
/// was read.
/// </summary>
public sealed class ConcurrencyConflictException(string entityName)
    : Exception(
        $"The {entityName} row was modified or deleted by another transaction. " +
        "Reload the aggregate and retry the operation."), IConcurrencyConflictException
{
    /// <summary>The table or entity whose row version did not match.</summary>
    public string EntityName { get; } = entityName;
}
