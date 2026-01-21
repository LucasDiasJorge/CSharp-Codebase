using System.Data;

namespace TransactionPattern.Core;

/// <summary>
/// Interface base para repositórios que suportam transações.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Inicia uma nova transação de banco de dados.
    /// </summary>
    /// <returns>Uma instância de IDbTransaction que representa a transação.</returns>
    IDbTransaction BeginTransaction();
}
