using System.Text.Json;

namespace Serialization;

class Program
{
    static void Main(string[] args)
    {
        var pessoaSerialize = new Pessoa { Nome = "Lucas", Idade = 22 };
        string jsonStringSerialize = JsonSerializer.Serialize(pessoaSerialize);
        
        Console.WriteLine(jsonStringSerialize);
        // Saída: {"Nome":"Lucas","Idade":22}
        
        string jsonStringDeserialize = "{\"Nome\":\"Maria\",\"Idade\":25}";

        Pessoa pessoaDeserialize = JsonSerializer.Deserialize<Pessoa>(jsonStringDeserialize);

        Console.WriteLine(pessoaDeserialize.Nome);  // Maria
        Console.WriteLine(pessoaDeserialize.Idade); // 25

    }
}
