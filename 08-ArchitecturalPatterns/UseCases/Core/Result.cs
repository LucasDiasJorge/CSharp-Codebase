namespace UseCases.Core;

/// <summary>
/// Classe para encapsular resultados de operações
/// Padrão Result para tratamento de erros sem exceções
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public IEnumerable<string> Errors { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = string.IsNullOrEmpty(error) ? [] : [error];
    }

    protected Result(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? [];
        Error = errors?.FirstOrDefault() ?? string.Empty;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

/// <summary>
/// Result genérico com valor de retorno
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, string.Empty)
    {
        Value = value;
    }

    private Result(string error) : base(false, error)
    {
        Value = default;
    }

    private Result(IEnumerable<string> errors) : base(false, errors)
    {
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string error) => new(error);
    public new static Result<T> Failure(IEnumerable<string> errors) => new(errors);

    // Implicit conversions
    public static implicit operator Result<T>(T value) => Success(value);
}
