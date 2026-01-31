using PersistencePatterns.Examples.IdentityMap.Entities;
using PersistencePatterns.Examples.IdentityMap.Interfaces;

namespace PersistencePatterns.Examples.IdentityMap.Implementations;

/// <summary>
/// Repositório com Identity Map integrado
/// Evita múltiplas buscas ao banco para a mesma entidade
/// </summary>
public class CustomerRepositoryWithIdentityMap : ICustomerRepository
{
    private readonly IIdentityMap<Customer, Guid> _identityMap;
    private readonly Dictionary<Guid, Customer> _database = []; // Simula banco
    private int _dbAccessCount;

    public CustomerRepositoryWithIdentityMap(IIdentityMap<Customer, Guid> identityMap)
    {
        _identityMap = identityMap;
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // 1. Verificar Identity Map primeiro
        var cached = _identityMap.Get(id);
        if (cached is not null)
        {
            Console.WriteLine($"  [IdentityMap] Cache HIT para {id}");
            return Task.FromResult<Customer?>(cached);
        }

        // 2. Buscar no "banco"
        Console.WriteLine($"  [IdentityMap] Cache MISS - Buscando no banco {id}");
        _dbAccessCount++;

        if (_database.TryGetValue(id, out var customer))
        {
            // 3. Adicionar ao Identity Map
            _identityMap.Add(id, customer);
            return Task.FromResult<Customer?>(customer);
        }

        return Task.FromResult<Customer?>(null);
    }

    public Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        // Busca por email precisa ir ao banco (não temos índice no cache)
        _dbAccessCount++;
        var customer = _database.Values.FirstOrDefault(c => c.Email == email);
        
        if (customer is not null)
        {
            _identityMap.Add(customer.Id, customer);
        }

        return Task.FromResult(customer);
    }

    public Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        _database[customer.Id] = customer;
        _identityMap.Add(customer.Id, customer);
        return Task.CompletedTask;
    }

    public void ClearCache()
    {
        _identityMap.Clear();
        Console.WriteLine("  [IdentityMap] Cache limpo");
    }

    public int DatabaseAccessCount => _dbAccessCount;
    
    public void ResetStats() => _dbAccessCount = 0;
}
