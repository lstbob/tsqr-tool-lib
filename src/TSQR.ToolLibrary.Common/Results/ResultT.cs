using TSQR.ToolLibrary.Common.Errors;

namespace TSQR.ToolLibrary.Common.Results;

public readonly struct Result<T>
{
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = Error.None;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Value = default!;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error Error { get; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
