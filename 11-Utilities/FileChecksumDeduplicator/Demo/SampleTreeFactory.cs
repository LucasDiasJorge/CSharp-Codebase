namespace FileChecksumDeduplicator.Demo;

/// <summary>
/// Monta a árvore de arquivos usada pelos cenários: muitos arquivos únicos, alguns
/// duplicados de verdade, um par de mesmo tamanho com conteúdos diferentes e um
/// arquivo grande.
///
/// Fica em <see cref="AppContext.BaseDirectory"/> porque <c>dotnet run --project</c>
/// mantém o diretório atual de quem chamou — caminho relativo aqui espalharia
/// arquivos pela raiz do repositório.
/// </summary>
public static class SampleTreeFactory
{
    public const int UniqueFileCount = 300;
    public const int DuplicatedContents = 4;
    public const int CopiesPerContent = 3;
    public const long LargeFileBytes = 32L * 1024 * 1024;

    public static string Directory => Path.Combine(AppContext.BaseDirectory, "dedup-demo");

    public static string LargeFilePath => Path.Combine(Directory, "grande.bin");

    public static string SameSizeFirstPath => Path.Combine(Directory, "mesmo-tamanho-a.bin");

    public static string SameSizeSecondPath => Path.Combine(Directory, "mesmo-tamanho-b.bin");

    /// <summary>
    /// Cria a árvore e devolve os caminhos que devem entrar na varredura.
    /// </summary>
    public static IReadOnlyList<string> Create()
    {
        Cleanup();
        System.IO.Directory.CreateDirectory(Directory);

        Random random = new Random(20260923);
        List<string> paths = new List<string>();

        // Arquivos unicos, todos com tamanhos distintos: nenhum deles precisa ser
        // lido para se saber que nao tem par.
        for (int index = 0; index < UniqueFileCount; index++)
        {
            string path = Path.Combine(Directory, $"unico-{index:D3}.bin");
            byte[] content = new byte[2_000 + (index * 37)];

            random.NextBytes(content);
            File.WriteAllBytes(path, content);
            paths.Add(path);
        }

        // Duplicatas reais: o mesmo conteudo em varios caminhos.
        for (int contentIndex = 0; contentIndex < DuplicatedContents; contentIndex++)
        {
            byte[] content = new byte[180_000 + contentIndex];

            random.NextBytes(content);

            for (int copy = 0; copy < CopiesPerContent; copy++)
            {
                string path = Path.Combine(Directory, $"copia-{contentIndex}-{copy}.bin");

                File.WriteAllBytes(path, content);
                paths.Add(path);
            }
        }

        // Mesmo tamanho, conteudo diferente: o unico byte de diferenca esta no FIM,
        // que e o pior caso para a comparacao byte a byte.
        byte[] first = new byte[64 * 1024];

        random.NextBytes(first);

        byte[] second = (byte[])first.Clone();

        second[^1] = (byte)(second[^1] ^ 0xFF);

        File.WriteAllBytes(SameSizeFirstPath, first);
        File.WriteAllBytes(SameSizeSecondPath, second);
        paths.Add(SameSizeFirstPath);
        paths.Add(SameSizeSecondPath);

        CreateLargeFile(random);
        paths.Add(LargeFilePath);

        return paths;
    }

    public static void Cleanup()
    {
        if (System.IO.Directory.Exists(Directory))
        {
            System.IO.Directory.Delete(Directory, recursive: true);
        }
    }

    /// <summary>
    /// Escreve o arquivo grande em blocos — o gerador também não pode montar 32 MB
    /// em memória de uma vez.
    /// </summary>
    private static void CreateLargeFile(Random random)
    {
        byte[] block = new byte[1024 * 1024];

        using FileStream stream = File.Create(LargeFilePath);

        for (long written = 0; written < LargeFileBytes; written += block.Length)
        {
            random.NextBytes(block);
            stream.Write(block, 0, block.Length);
        }
    }
}
