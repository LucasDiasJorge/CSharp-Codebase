using CachePatterns.Models;

namespace CachePatterns.Data;

// Simulação de um banco de dados
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task SaveAsync(T entity);
    Task DeleteAsync(int id);
}

public class ProdutoRepository : IRepository<Produto>
{
    private static readonly Dictionary<int, Produto> _produtos = new()
    {
        { 1, new Produto { Id = 1, Nome = "Notebook Dell", Preco = 2500.00m, Categoria = "Eletrônicos", UltimaAtualizacao = DateTime.Now } },
        { 2, new Produto { Id = 2, Nome = "Mouse Logitech", Preco = 150.00m, Categoria = "Periféricos", UltimaAtualizacao = DateTime.Now } },
        { 3, new Produto { Id = 3, Nome = "Teclado Mecânico", Preco = 300.00m, Categoria = "Periféricos", UltimaAtualizacao = DateTime.Now } },
        { 4, new Produto { Id = 4, Nome = "Monitor 24\"", Preco = 800.00m, Categoria = "Eletrônicos", UltimaAtualizacao = DateTime.Now } },
        { 5, new Produto { Id = 5, Nome = "Smartphone", Preco = 1200.00m, Categoria = "Eletrônicos", UltimaAtualizacao = DateTime.Now } }
    };

    public async Task<Produto?> GetByIdAsync(int id)
    {
        // Simula latência do banco de dados
        await Task.Delay(100);
        
        Console.WriteLine($"[DB] Buscando produto ID: {id}");
        return _produtos.TryGetValue(id, out var produto) ? produto : null;
    }

    public async Task<IEnumerable<Produto>> GetAllAsync()
    {
        await Task.Delay(200);
        Console.WriteLine("[DB] Buscando todos os produtos");
        return _produtos.Values.ToList();
    }

    public async Task SaveAsync(Produto produto)
    {
        await Task.Delay(50);
        Console.WriteLine($"[DB] Salvando produto: {produto.Nome}");
        produto.UltimaAtualizacao = DateTime.Now;
        _produtos[produto.Id] = produto;
    }

    public async Task DeleteAsync(int id)
    {
        await Task.Delay(50);
        Console.WriteLine($"[DB] Deletando produto ID: {id}");
        _produtos.Remove(id);
    }
}

public class UsuarioRepository : IRepository<Usuario>
{
    private static readonly Dictionary<int, Usuario> _usuarios = new()
    {
        { 1, new Usuario { Id = 1, Nome = "João Silva", Email = "joao@email.com", UltimoAcesso = DateTime.Now } },
        { 2, new Usuario { Id = 2, Nome = "Maria Santos", Email = "maria@email.com", UltimoAcesso = DateTime.Now } },
        { 3, new Usuario { Id = 3, Nome = "Pedro Oliveira", Email = "pedro@email.com", UltimoAcesso = DateTime.Now } }
    };

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        await Task.Delay(80);
        Console.WriteLine($"[DB] Buscando usuário ID: {id}");
        return _usuarios.TryGetValue(id, out var usuario) ? usuario : null;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        await Task.Delay(150);
        Console.WriteLine("[DB] Buscando todos os usuários");
        return _usuarios.Values.ToList();
    }

    public async Task SaveAsync(Usuario usuario)
    {
        await Task.Delay(40);
        Console.WriteLine($"[DB] Salvando usuário: {usuario.Nome}");
        usuario.UltimoAcesso = DateTime.Now;
        _usuarios[usuario.Id] = usuario;
    }

    public async Task DeleteAsync(int id)
    {
        await Task.Delay(40);
        Console.WriteLine($"[DB] Deletando usuário ID: {id}");
        _usuarios.Remove(id);
    }
}
