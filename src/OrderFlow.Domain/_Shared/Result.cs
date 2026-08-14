namespace OrderFlow.Domain._Shared;

public class Result<T>
{
    protected Result(bool isSuccess, Error? error, T? value)
    {
        IsSuccess = isSuccess;
        Error     = error;
        Value     = value;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public T Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<T> Success(T value) => new(true, null, value);

    public static Result<T> Failure(Error error) => new(false, error, default);
}

public class Result : Result<object?>
{
    private Result(bool isSuccess, Error? error) : base(isSuccess, error, null) { }

    public static Result Success() => new(true, null);

    public new static Result Failure(Error error) => new(false, error);
}
