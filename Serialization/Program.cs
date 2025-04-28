using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using MessagePack;

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
        
        var lucas = new Pessoa { Nome = "Lucas", Idade = 22 };

        // Serializar para binário
        byte[] binario = MessagePackSerializer.Serialize(lucas);

        // Gravar no arquivo
        File.WriteAllBytes("pessoa.msgpack", binario);

        // Ler do arquivo
        byte[] binarioLido = File.ReadAllBytes("pessoa.msgpack");

        // Desserializar
        Pessoa pessoaDesserializada = MessagePackSerializer.Deserialize<Pessoa>(binarioLido);

        Console.WriteLine(pessoaDesserializada.Nome); // Lucas
        Console.WriteLine(pessoaDesserializada.Idade); // 22
        
        /*
        // --- Binary Serialization ---
        var pessoaBinary = new Pessoa { Nome = "Ana", Idade = 25 };

        try
        {
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var stream = new FileStream("pessoa.bin", FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, pessoaBinary);
            }

            Pessoa deserializedFromBinary;
            using (var stream = new FileStream("pessoa.bin", FileMode.Open, FileAccess.Read))
            {
                deserializedFromBinary = (Pessoa)formatter.Deserialize(stream);
            }

            Console.WriteLine(deserializedFromBinary.Nome); // Ana
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro de IO: {ex.Message}");
        }

        */
        /*
            OBS: BinaryFormatter é obsoleto!
            Prefira System.Text.Json para salvar como texto (talvez usando Base64 para converter objetos complexos).
            Ou use MessagePack, Protobuf para serialização binária segura e eficiente.
        */
    }
}
