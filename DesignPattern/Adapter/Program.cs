using Adapter.Adapters;
using Adapter.Interfaces;
using Adapter.Models;

namespace Adapter;

/// <summary>
/// Demonstração do padrão Adapter
/// Mostra como integrar um sistema legado com interface moderna
/// </summary>
class Program
{
    static void Main()
    {
        Console.WriteLine("=== Demonstração do Padrão Adapter ===");
        Console.WriteLine("Integrando sistema legado com interface moderna\n");

        Console.WriteLine("1. Criando repositório com adapter:");
        IClientRepository repository = new ClientRepositoryAdapter();

        Console.WriteLine("\n2. Adicionando clientes através da interface moderna:");
        repository.AddClient(new Client { Name = "Lucas", Age = 22 });
        repository.AddClient(new Client { Name = "Maria", Age = 30 });
        repository.AddClient(new Client { Name = "João", Age = 25 });

        Console.WriteLine("\n3. Recuperando clientes através da interface moderna:");
        var clients = repository.GetAllClients();
        
        Console.WriteLine("\n4. Listando clientes:");
        foreach (var client in clients)
        {
            Console.WriteLine($"  • {client}");
        }

        Console.WriteLine("\n5. Demonstrando funcionalidade adicional do adapter:");
        if (repository is ClientRepositoryAdapter adapter)
        {
            Console.WriteLine($"  Total de clientes: {adapter.GetClientCount()}");
        }

        Console.WriteLine("\n=== Benefícios do Adapter ===");
        Console.WriteLine("✅ Sistema legado integrado sem modificações");
        Console.WriteLine("✅ Interface moderna mantida consistente");
        Console.WriteLine("✅ Conversões de dados centralizadas no adapter");
        Console.WriteLine("✅ Facilita testes e manutenção");

        Console.WriteLine("\n=== Fim da Demonstração ===");
        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}
