using MessagePack;

namespace Serialization;

[MessagePackObject]
public class Pessoa
{
    [Key(0)]
    public string Nome { get; set; }

    [Key(1)]
    public int Idade { get; set; }
}