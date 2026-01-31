namespace TransactionScript.Core;

/// <summary>
/// Interface base para Transaction Scripts
/// </summary>
public interface ITransactionScript<TInput, TOutput>
{
    ScriptResult<TOutput> Execute(TInput input);
}

/// <summary>
/// Interface para scripts sem retorno
/// </summary>
public interface ITransactionScript<TInput>
{
    ScriptResult Execute(TInput input);
}
