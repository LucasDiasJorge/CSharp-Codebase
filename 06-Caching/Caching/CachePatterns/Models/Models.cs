namespace CachePatterns.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public DateTime UltimaAtualizacao { get; set; }
    
    public override string ToString()
    {
        return $"Produto: {Nome} - Preço: {Preco:C} - Categoria: {Categoria}";
    }
}

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime UltimoAcesso { get; set; }
}

public class Configuracao
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public DateTime UltimaAtualizacao { get; set; }
}
