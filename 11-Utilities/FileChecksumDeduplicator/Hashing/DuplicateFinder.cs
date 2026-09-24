using System.Diagnostics;

namespace FileChecksumDeduplicator.Hashing;

/// <summary>
/// Um grupo de arquivos com o mesmo conteúdo.
/// </summary>
public sealed record DuplicateGroup(string Hash, long Size, IReadOnlyList<string> Paths);

/// <summary>
/// Resultado de uma varredura, com o custo de I/O pago para chegar nele. É a
/// comparação desses custos que justifica a estratégia.
/// </summary>
public sealed record ScanResult(
    IReadOnlyList<DuplicateGroup> Groups,
    int FilesHashed,
    long BytesRead,
    double ElapsedMilliseconds);

/// <summary>
/// Duas estratégias para achar duplicatas. A diferença entre elas não é o resultado
/// — é quanto disco precisou ser lido para chegar nele.
/// </summary>
public static class DuplicateFinder
{
    /// <summary>
    /// Estratégia ingênua: calcula o hash de todo arquivo e agrupa. Lê o conteúdo
    /// inteiro de todos eles, inclusive dos que nem poderiam ter par.
    /// </summary>
    public static ScanResult ScanHashingEverything(IReadOnlyList<string> paths)
    {
        Stopwatch watch = Stopwatch.StartNew();
        Dictionary<string, List<string>> byHash = new Dictionary<string, List<string>>();
        long bytesRead = 0;
        int filesHashed = 0;

        foreach (string path in paths)
        {
            HashResult result = FileHasher.ComputeStreaming(path);

            bytesRead += result.BytesRead;
            filesHashed++;

            if (!byHash.TryGetValue(result.Hash, out List<string>? group))
            {
                group = new List<string>();
                byHash[result.Hash] = group;
            }

            group.Add(path);
        }

        return BuildResult(byHash, filesHashed, bytesRead, watch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Estratégia por tamanho primeiro: dois arquivos de tamanhos diferentes não
    /// podem ter o mesmo conteúdo, e o tamanho vem dos metadados — sem ler um byte.
    /// Só os arquivos que compartilham tamanho com algum outro chegam a ser lidos.
    /// </summary>
    public static ScanResult ScanGroupingBySizeFirst(IReadOnlyList<string> paths)
    {
        Stopwatch watch = Stopwatch.StartNew();
        Dictionary<long, List<string>> bySize = new Dictionary<long, List<string>>();

        foreach (string path in paths)
        {
            long size = new FileInfo(path).Length;

            if (!bySize.TryGetValue(size, out List<string>? group))
            {
                group = new List<string>();
                bySize[size] = group;
            }

            group.Add(path);
        }

        Dictionary<string, List<string>> byHash = new Dictionary<string, List<string>>();
        long bytesRead = 0;
        int filesHashed = 0;

        foreach (KeyValuePair<long, List<string>> sizeGroup in bySize)
        {
            // Tamanho unico: nao existe candidato a par, entao o arquivo nunca e lido.
            if (sizeGroup.Value.Count < 2)
            {
                continue;
            }

            foreach (string path in sizeGroup.Value)
            {
                HashResult result = FileHasher.ComputeStreaming(path);

                bytesRead += result.BytesRead;
                filesHashed++;

                if (!byHash.TryGetValue(result.Hash, out List<string>? group))
                {
                    group = new List<string>();
                    byHash[result.Hash] = group;
                }

                group.Add(path);
            }
        }

        return BuildResult(byHash, filesHashed, bytesRead, watch.Elapsed.TotalMilliseconds);
    }

    private static ScanResult BuildResult(
        Dictionary<string, List<string>> byHash,
        int filesHashed,
        long bytesRead,
        double elapsed)
    {
        List<DuplicateGroup> groups = new List<DuplicateGroup>();

        foreach (KeyValuePair<string, List<string>> entry in byHash)
        {
            if (entry.Value.Count < 2)
            {
                continue;
            }

            groups.Add(new DuplicateGroup(entry.Key, new FileInfo(entry.Value[0]).Length, entry.Value));
        }

        groups.Sort((first, second) => second.Paths.Count.CompareTo(first.Paths.Count));

        return new ScanResult(groups, filesHashed, bytesRead, elapsed);
    }
}
