using System.Diagnostics;
using System.Security.Cryptography;

namespace FileChecksumDeduplicator.Hashing;

/// <summary>
/// Resultado de um hash, com o custo pago para obtê-lo. É a leitura de bytes que
/// domina o tempo — por isso ela é contada.
/// </summary>
public sealed record HashResult(string Hash, long BytesRead, double ElapsedMilliseconds);

/// <summary>
/// Hash por stream: lê o arquivo em blocos e alimenta o algoritmo incrementalmente,
/// sem nunca ter o conteúdo inteiro em memória.
///
/// <see cref="IncrementalHash"/> é o que permite isso — o estado do hash cabe em
/// dezenas de bytes, independentemente do tamanho do arquivo.
/// </summary>
public static class FileHasher
{
    public const int DefaultBufferSize = 64 * 1024;

    /// <summary>
    /// Calcula o SHA-256 lendo em blocos de <paramref name="bufferSize"/> bytes.
    /// A memória usada é o buffer, não o arquivo.
    /// </summary>
    public static HashResult ComputeStreaming(string path, int bufferSize = DefaultBufferSize)
    {
        Stopwatch watch = Stopwatch.StartNew();
        long bytesRead = 0;

        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using FileStream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize,
            FileOptions.SequentialScan);

        byte[] buffer = new byte[bufferSize];
        int read;

        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            hash.AppendData(buffer, 0, read);
            bytesRead += read;
        }

        return new HashResult(
            Convert.ToHexString(hash.GetHashAndReset()),
            bytesRead,
            watch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// A alternativa ingênua: carregar o arquivo inteiro para depois calcular o hash.
    /// Dá o mesmo resultado e cobra o tamanho do arquivo em memória.
    /// </summary>
    public static HashResult ComputeLoadingEverything(string path)
    {
        Stopwatch watch = Stopwatch.StartNew();

        byte[] content = File.ReadAllBytes(path);
        byte[] digest = SHA256.HashData(content);

        return new HashResult(
            Convert.ToHexString(digest),
            content.LongLength,
            watch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Comparação byte a byte, para quando "hash igual" não basta. É o desempate
    /// definitivo: só devolve verdadeiro se os arquivos forem idênticos de fato.
    /// </summary>
    public static bool ContentsAreEqual(string firstPath, string secondPath, int bufferSize = DefaultBufferSize)
    {
        using FileStream first = File.OpenRead(firstPath);
        using FileStream second = File.OpenRead(secondPath);

        if (first.Length != second.Length)
        {
            return false;
        }

        byte[] firstBuffer = new byte[bufferSize];
        byte[] secondBuffer = new byte[bufferSize];

        while (true)
        {
            int firstRead = first.Read(firstBuffer, 0, bufferSize);
            int secondRead = second.Read(secondBuffer, 0, bufferSize);

            if (firstRead != secondRead)
            {
                return false;
            }

            if (firstRead == 0)
            {
                return true;
            }

            if (!firstBuffer.AsSpan(0, firstRead).SequenceEqual(secondBuffer.AsSpan(0, secondRead)))
            {
                return false;
            }
        }
    }
}
