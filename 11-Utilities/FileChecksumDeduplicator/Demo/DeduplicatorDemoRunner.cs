using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using FileChecksumDeduplicator.Hashing;
using Microsoft.Extensions.Logging;

namespace FileChecksumDeduplicator.Demo;

/// <summary>
/// Cinco cenários: hash por stream contra carregar tudo, achar duplicatas com e sem
/// filtro por tamanho, o que o tamanho e o hash garantem (e o que não garantem),
/// colisões medidas por aniversário e o efeito do tamanho do buffer.
/// </summary>
public sealed class DeduplicatorDemoRunner
{
    private readonly ILogger<DeduplicatorDemoRunner> _logger;

    public DeduplicatorDemoRunner(ILogger<DeduplicatorDemoRunner> logger)
    {
        _logger = logger;
    }

    public void RunAll()
    {
        Section("Preparando a arvore de arquivos");

        Stopwatch watch = Stopwatch.StartNew();
        IReadOnlyList<string> paths = SampleTreeFactory.Create();

        _logger.LogInformation(
            "  {Arquivos} arquivos criados em {Tempo}ms ({Total:N1} MB), inclusive um de {Grande} MB",
            paths.Count,
            watch.ElapsedMilliseconds,
            paths.Sum(path => new FileInfo(path).Length) / 1024.0 / 1024.0,
            SampleTreeFactory.LargeFileBytes / 1024 / 1024);

        RunStreamingVersusLoading();
        RunScanStrategies(paths);
        RunSizeAndHashGuarantees();
        RunCollisionProbability();
        RunBufferSizes();

        SampleTreeFactory.Cleanup();
    }

    private void RunStreamingVersusLoading()
    {
        Section("1. Hash por stream contra carregar o arquivo inteiro");

        string path = SampleTreeFactory.LargeFilePath;

        // Medir memoria RETIDA aqui nao mostra nada: os dois metodos soltam tudo ao
        // retornar. O que separa as duas estrategias e quanto foi ALOCADO no caminho.
        Collect();
        long beforeStreaming = GC.GetTotalAllocatedBytes(precise: true);
        HashResult streaming = FileHasher.ComputeStreaming(path);
        long streamingAllocated = GC.GetTotalAllocatedBytes(precise: true) - beforeStreaming;

        Collect();
        long beforeLoading = GC.GetTotalAllocatedBytes(precise: true);
        HashResult loading = FileHasher.ComputeLoadingEverything(path);
        long loadingAllocated = GC.GetTotalAllocatedBytes(precise: true) - beforeLoading;

        _logger.LogInformation("  arquivo de {Tamanho} MB", SampleTreeFactory.LargeFileBytes / 1024 / 1024);

        _logger.LogInformation(
            "  streaming (buffer de 64 KB): {Memoria} MB alocados, {Tempo:F1}ms",
            FormatMegabytes(streamingAllocated),
            streaming.ElapsedMilliseconds);

        _logger.LogInformation(
            "  ReadAllBytes + HashData:     {Memoria} MB alocados, {Tempo:F1}ms",
            FormatMegabytes(loadingAllocated),
            loading.ElapsedMilliseconds);

        _logger.LogInformation(
            "  mesmo hash nos dois: {Igual} ({Hash}...)",
            streaming.Hash == loading.Hash,
            streaming.Hash[..16]);

        _logger.LogInformation(
            "O resultado e identico. A diferenca e que a versao por stream tem custo de memoria FIXO — ela processaria um arquivo de 50 GB com o mesmo buffer de 64 KB.");
    }

    private void RunScanStrategies(IReadOnlyList<string> paths)
    {
        Section("2. Achar duplicatas: o tamanho vem de graca, o conteudo nao");

        ScanResult naive = DuplicateFinder.ScanHashingEverything(paths);
        ScanResult smart = DuplicateFinder.ScanGroupingBySizeFirst(paths);

        _logger.LogInformation(
            "  hashear tudo:        {Hasheados,4} arquivos lidos, {Bytes,8:N1} MB, {Tempo,6:F1}ms -> {Grupos} grupos",
            naive.FilesHashed,
            naive.BytesRead / 1024.0 / 1024.0,
            naive.ElapsedMilliseconds,
            naive.Groups.Count);

        _logger.LogInformation(
            "  agrupar por tamanho: {Hasheados,4} arquivos lidos, {Bytes,8:N1} MB, {Tempo,6:F1}ms -> {Grupos} grupos",
            smart.FilesHashed,
            smart.BytesRead / 1024.0 / 1024.0,
            smart.ElapsedMilliseconds,
            smart.Groups.Count);

        _logger.LogInformation("  os dois acharam os mesmos grupos: {Igual}", SameGroups(naive, smart));

        foreach (DuplicateGroup group in smart.Groups)
        {
            _logger.LogInformation(
                "    {Copias} copias de {Tamanho:N0} bytes: {Arquivos}",
                group.Paths.Count,
                group.Size,
                string.Join(", ", group.Paths.Select(Path.GetFileName)));
        }

        _logger.LogInformation(
            "O tamanho esta nos metadados do sistema de arquivos: obte-lo nao le um byte do conteudo. Arquivo com tamanho unico nao pode ter par, entao nunca precisa ser aberto.");
    }

    private void RunSizeAndHashGuarantees()
    {
        Section("3. O que o tamanho garante, o que o hash garante");

        string first = SampleTreeFactory.SameSizeFirstPath;
        string second = SampleTreeFactory.SameSizeSecondPath;

        long firstSize = new FileInfo(first).Length;
        long secondSize = new FileInfo(second).Length;

        HashResult firstHash = FileHasher.ComputeStreaming(first);
        HashResult secondHash = FileHasher.ComputeStreaming(second);

        _logger.LogWarning(
            "  mesmo-tamanho-a e mesmo-tamanho-b: {Tamanho:N0} bytes os dois (iguais: {Iguais})",
            firstSize,
            firstSize == secondSize);

        _logger.LogInformation(
            "    hash a: {HashA}... | hash b: {HashB}... | iguais: {Iguais}",
            firstHash.Hash[..16],
            secondHash.Hash[..16],
            firstHash.Hash == secondHash.Hash);

        _logger.LogInformation(
            "  um unico byte de diferenca, no FIM do arquivo, muda o hash inteiro (efeito avalanche).");

        // Confirmacao byte a byte entre duas copias de verdade.
        string copyA = Path.Combine(SampleTreeFactory.Directory, "copia-0-0.bin");
        string copyB = Path.Combine(SampleTreeFactory.Directory, "copia-0-1.bin");

        Stopwatch watch = Stopwatch.StartNew();
        bool identical = FileHasher.ContentsAreEqual(copyA, copyB);

        _logger.LogInformation(
            "  copia-0-0 x copia-0-1, comparacao byte a byte: {Resultado} em {Tempo:F1}ms",
            identical,
            watch.Elapsed.TotalMilliseconds);

        watch.Restart();
        bool differentPair = FileHasher.ContentsAreEqual(first, second);

        _logger.LogInformation(
            "  mesmo-tamanho-a x mesmo-tamanho-b, byte a byte: {Resultado} em {Tempo:F1}ms (a diferenca so aparece no ultimo bloco)",
            differentPair,
            watch.Elapsed.TotalMilliseconds);
    }

    private void RunCollisionProbability()
    {
        Section("4. Colisao: por que 256 bits e por que nao 32");

        _logger.LogInformation("  quantos arquivos ate duas somas baterem, truncando o SHA-256:");

        foreach (int bits in new[] { 16, 24, 32 })
        {
            int attempts = FindFirstCollision(bits, out string collidingHash);
            double expected = 1.2533 * Math.Pow(2, bits / 2.0);

            _logger.LogWarning(
                "    {Bits,2} bits: colidiu em {Tentativas,9:N0} entradas (esperado ~{Esperado,9:N0}) no valor {Valor}",
                bits,
                attempts,
                expected,
                collidingHash);
        }

        _logger.LogInformation(
            "  o esperado e ~1,25 x raiz(2^bits) — o paradoxo do aniversario: a colisao chega na RAIZ do espaco, nao na metade dele.");

        double sha256Expected = 1.2533 * Math.Pow(2, 128);

        _logger.LogInformation(
            "  para 256 bits o mesmo calculo da ~{Esperado:E2} arquivos. E por isso que SHA-256 e tratado como identidade de conteudo.",
            sha256Expected);

        _logger.LogWarning(
            "  MD5 e SHA-1 tem 128 e 160 bits, mas o problema deles nao e o tamanho: sao colisoes CONSTRUIDAS, publicadas, baratas. Nao servem onde alguem pode escolher o conteudo.");
    }

    private void RunBufferSizes()
    {
        Section("5. Tamanho do buffer: o custo de I/O aparece aqui");

        string path = SampleTreeFactory.LargeFilePath;

        // Primeira leitura so para aquecer o cache do sistema de arquivos — senao a
        // primeira medicao paga o disco e as outras nao.
        FileHasher.ComputeStreaming(path);

        foreach (int bufferSize in new[] { 1024, 4 * 1024, 64 * 1024, 1024 * 1024 })
        {
            HashResult result = FileHasher.ComputeStreaming(path, bufferSize);
            long reads = (long)Math.Ceiling((double)result.BytesRead / bufferSize);
            double throughput = result.BytesRead / 1024.0 / 1024.0 / (result.ElapsedMilliseconds / 1000.0);

            _logger.LogInformation(
                "  buffer {Buffer,7}: {Leituras,8:N0} chamadas de Read, {Tempo,6:F1}ms, {Vazao,7:N0} MB/s",
                FormatBytes(bufferSize),
                reads,
                result.ElapsedMilliseconds,
                throughput);
        }

        _logger.LogInformation(
            "Buffer pequeno demais paga uma chamada de sistema por bloco; grande demais so ocupa memoria sem ganho. Dezenas de KB costumam ser o ponto de equilibrio.");
    }

    /// <summary>
    /// Procura a primeira colisão truncando o SHA-256 em <paramref name="bits"/> bits.
    /// Serve para medir, com números pequenos, a lei que vale para os grandes.
    /// </summary>
    private static int FindFirstCollision(int bits, out string collidingHash)
    {
        Dictionary<string, int> seen = new Dictionary<string, int>();
        int hexDigits = bits / 4;

        for (int attempt = 1; attempt < 50_000_000; attempt++)
        {
            byte[] content = Encoding.UTF8.GetBytes($"arquivo-{attempt}");
            string truncated = Convert.ToHexString(SHA256.HashData(content))[..hexDigits];

            if (seen.ContainsKey(truncated))
            {
                collidingHash = truncated;

                return attempt;
            }

            seen[truncated] = attempt;
        }

        collidingHash = "nenhuma";

        return -1;
    }

    private static bool SameGroups(ScanResult first, ScanResult second)
    {
        HashSet<string> firstHashes = first.Groups.Select(group => group.Hash).ToHashSet();
        HashSet<string> secondHashes = second.Groups.Select(group => group.Hash).ToHashSet();

        return firstHashes.SetEquals(secondHashes);
    }

    /// <summary>
    /// Coleta de verdade antes de medir. Uma chamada só a <c>GC.GetTotalMemory</c>
    /// pode deixar para trás objetos recém-soltos, e a medição sai negativa.
    /// </summary>
    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static string FormatMegabytes(long bytes)
    {
        double megabytes = bytes / 1024.0 / 1024.0;

        return Math.Abs(megabytes) < 0.1 ? "~0,0" : megabytes.ToString("N1");
    }

    private static string FormatBytes(int bytes) =>
        bytes >= 1024 * 1024 ? $"{bytes / 1024 / 1024} MB" : $"{bytes / 1024} KB";

    private static void Section(string title)
    {
        Thread.Sleep(120);

        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
