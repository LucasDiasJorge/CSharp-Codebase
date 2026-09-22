using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCoreRelationshipsDemo.Data;

/// <summary>
/// Conta as consultas que chegam ao banco. É o instrumento que transforma "cuidado com
/// N+1" em um número — sem contar, a diferença entre uma consulta e vinte e uma passa
/// despercebida até a produção.
/// </summary>
public sealed class QueryCounter : DbCommandInterceptor
{
    private int _count;

    public int Count => Volatile.Read(ref _count);

    public void Reset() => Interlocked.Exchange(ref _count, 0);

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Interlocked.Increment(ref _count);

        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _count);

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
