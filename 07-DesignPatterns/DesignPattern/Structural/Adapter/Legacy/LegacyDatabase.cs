namespace Adapter.Legacy;

/// <summary>
/// Classe Adaptee: Sistema legado que precisa ser integrado
/// Representa um banco de dados antigo com interface incompatível
/// </summary>
public class LegacyDatabase
{
    private readonly List<Dictionary<string, object>> _storage = new();

    /// <summary>
    /// Método legado que insere registros como dicionários
    /// </summary>
    public void Insert(Dictionary<string, object> record)
    {
        _storage.Add(record);
        Console.WriteLine($"[Legacy DB] Inserted client: {record["Name"]}");
    }

    /// <summary>
    /// Método legado que retorna todos os registros como dicionários
    /// </summary>
    public List<Dictionary<string, object>> FetchAll()
    {
        Console.WriteLine($"[Legacy DB] Fetching {_storage.Count} records");
        return _storage;
    }

    /// <summary>
    /// Simula uma operação específica do sistema legado
    /// </summary>
    public int GetRecordCount()
    {
        return _storage.Count;
    }
}
