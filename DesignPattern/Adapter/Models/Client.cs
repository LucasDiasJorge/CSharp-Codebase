namespace Adapter.Models;

/// <summary>
/// Classe de domínio que representa um cliente
/// </summary>
public class Client
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }

    public override string ToString()
    {
        return $"{Name}, {Age} years old";
    }
}
