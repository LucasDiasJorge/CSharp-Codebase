namespace TransactionScript.Core;

/// <summary>
/// Resultado de uma operação de script
/// </summary>
public class ScriptResult
{
    public bool Success { get; }
    public string? Error { get; }
    public object? Data { get; }

    private ScriptResult(bool success, string? error, object? data)
    {
        Success = success;
        Error = error;
        Data = data;
    }

    public static ScriptResult Ok(object? data = null) => new(true, null, data);
    public static ScriptResult Fail(string error) => new(false, error, null);
}

/// <summary>
/// Resultado tipado
/// </summary>
public class ScriptResult<T>
{
    public bool Success { get; }
    public string? Error { get; }
    public T? Data { get; }

    private ScriptResult(bool success, string? error, T? data)
    {
        Success = success;
        Error = error;
        Data = data;
    }

    public static ScriptResult<T> Ok(T data) => new(true, null, data);
    public static ScriptResult<T> Fail(string error) => new(false, error, default);
}
