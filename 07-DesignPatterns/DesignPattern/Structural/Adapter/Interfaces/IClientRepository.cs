using Adapter.Models;

namespace Adapter.Interfaces;

/// <summary>
/// Interface target que nosso código moderno espera usar
/// Define operações padrão para repositório de clientes
/// </summary>
public interface IClientRepository
{
    void AddClient(Client client);
    List<Client> GetAllClients();
}
