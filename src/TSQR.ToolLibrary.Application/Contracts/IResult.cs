namespace TSQR.ToolLibrary.Application.Contracts;

/// <summary>
/// Represents contract for implementation for an operation result.
/// </summary>
public interface IResult
{
    public ResultType ResultType { get; }
    public string? Message { get; }
}

/// <summary>
/// Represents description of command and queries result types.
/// </summary>
public enum ResultType
{
    /// <summary>
    /// Command or Query was executed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The resource requested by the Query is missing.
    /// </summary>
    ResoruceMissing,

    /// <summary>
    /// The resource was created successfully.
    /// </summary>
    ResourceCreated,

    /// <summary>
    /// The resource was updated successfully.
    /// </summary>
    ResourceUpdated,

    /// <summary>
    /// The resource was removed successfully.
    /// </summary>
    ResourceRemoved,

    /// <summary>
    /// The Command or Query operation requested was invalid or not part of the API contract.
    /// </summary>
    InvaildOperationRequested,

    /// <summary>
    /// The Command or Query operation requested had invalid input.
    /// </summary>
    InvalidInput,

    /// <summary>
    /// There was an unexpected failure in the Command or Query that was not handled by the system.
    /// </summary>
    UnhandledError,
}
