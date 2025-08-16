using Adapter.Interfaces;
using Adapter.Legacy;
using Adapter.Models;

namespace Adapter.Adapters;

/// <summary>
/// Adapter: Traduz a interface moderna (IClientRepository) 
/// para o sistema legado (LegacyDatabase)
/// </summary>
public class ClientRepositoryAdapter : IClientRepository
{
    private readonly LegacyDatabase _legacyDb;

    public ClientRepositoryAdapter()
    {
        _legacyDb = new LegacyDatabase();
        Console.WriteLine("[Adapter] Initialized with legacy database");
    }

    /// <summary>
    /// Adapta AddClient para o método Insert do sistema legado
    /// </summary>
    public void AddClient(Client client)
    {
        if (client == null) 
            throw new ArgumentNullException(nameof(client));

        // Converte Client para formato esperado pelo sistema legado
        var record = new Dictionary<string, object>
        {
            { "Name", client.Name },
            { "Age", client.Age },
            { "CreatedAt", DateTime.Now }
        };

        _legacyDb.Insert(record);
    }

    /// <summary>
    /// Adapta GetAllClients para o método FetchAll do sistema legado
    /// </summary>
    public List<Client> GetAllClients()
    {
        var legacyRecords = _legacyDb.FetchAll();
        var clients = new List<Client>();

        foreach (var record in legacyRecords)
        {
            // Converte formato legado de volta para Client
            var client = new Client
            {
                Name = record["Name"]?.ToString() ?? "Unknown",
                Age = Convert.ToInt32(record["Age"])
            };
            clients.Add(client);
        }

        Console.WriteLine($"[Adapter] Converted {clients.Count} legacy records to Client objects");
        return clients;
    }

    /// <summary>
    /// Método adicional que expõe funcionalidade específica do sistema legado
    /// </summary>
    public int GetClientCount()
    {
        return _legacyDb.GetRecordCount();
    }
}
