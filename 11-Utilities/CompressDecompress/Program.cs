using System;
using System.IO;
using System.IO.Compression;
using System.Text;

class Program
{
    static void Main()
    {
        Console.WriteLine("Digite o texto a ser comprimido:");
        string input = Console.ReadLine();

        // Comprime a string
        byte[] compressedData = CompressString(input);
        Console.WriteLine("\nDados comprimidos (em base64):");
        Console.WriteLine(Convert.ToBase64String(compressedData));

        // Descomprime e mostra o resultado
        string decompressed = DecompressToString(compressedData);
        Console.WriteLine("\nTexto descomprimido:");
        Console.WriteLine(decompressed);
    }

    // Compressão
    static byte[] CompressString(string text)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(text);
        using (var outputStream = new MemoryStream())
        {
            using (var gzip = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzip.Write(inputBytes, 0, inputBytes.Length);
            }
            return outputStream.ToArray();
        }
    }

    // Descompressão
    static string DecompressToString(byte[] compressedData)
    {
        using (var inputStream = new MemoryStream(compressedData))
        using (var gzip = new GZipStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new MemoryStream())
        {
            gzip.CopyTo(outputStream);
            return Encoding.UTF8.GetString(outputStream.ToArray());
        }
    }
}
