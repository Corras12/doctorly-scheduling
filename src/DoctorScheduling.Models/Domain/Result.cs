namespace DoctorScheduling.Models.Domain;

public enum ResultType
{
    Success,
    ValidationError,
    NotFound,
    Conflict
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultType Type { get; }

    private Result(bool isSuccess, T? value, string? error, ResultType type)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Type = type;
    }

    public static Result<T> Success(T value) => new(true, value, null, ResultType.Success);
    public static Result<T> Failure(string error, ResultType type = ResultType.ValidationError) =>
        new(false, default, error, type);
    public static Result<T> NotFound(string error) => new(false, default, error, ResultType.NotFound);
    public static Result<T> ConflictFailure(string error) => new(false, default, error, ResultType.Conflict);
}
