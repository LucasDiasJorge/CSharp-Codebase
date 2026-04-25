# CompressDecompress

## Visão geral

Exemplo de compressão e descompressão de strings usando GZipStream do .NET.

## Conceitos abordados

- **GZipStream**: Stream de compressão GZIP nativo do .NET
- **MemoryStream**: Manipulação de dados em memória
- **Encoding UTF-8**: Conversão entre strings e bytes
- **Base64**: Codificação de dados binários para texto

## Objetivos de aprendizagem

- Implementar compressão e descompressão de dados em C#
- Entender o uso de streams encadeados
- Converter entre representações de dados (string, bytes, base64)
- Aplicar using para gerenciamento correto de recursos

## Estrutura do projeto

```text
CompressDecompress/
+-- CompressDecompress.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 11-Utilities/CompressDecompress/CompressDecompress.csproj
```

## Boas práticas e pontos de atenção

- Usar `using` para garantir dispose dos streams
- Preferir `CompressionLevel.Optimal` para melhor compressão
- Validar dados antes de descomprimir
- Considerar BrotliStream para compressão web

### Pontos de Atenção

- GZip adiciona overhead - não comprimir dados já pequenos
- Dados aleatórios/já comprimidos podem aumentar de tamanho
- Streams devem ser fechados na ordem correta

## Conteúdo complementar

##### Estrutura do Projeto

```
CompressDecompress/
├── Program.cs
├── CompressDecompress.csproj
└── README.md
```

##### Exemplo de Uso

```
Digite o texto a ser comprimido:
> Este é um texto de exemplo para demonstrar a compressão com GZip.

Dados comprimidos (em base64):
H4sIAAAAAAAAA0tJLS4pysxLT8nMS1dILEpVSMsvyklRBAA=

Texto descomprimido:
Este é um texto de exemplo para demonstrar a compressão com GZip.
```

##### Compressão

```csharp
static byte[] CompressString(string text)
{
    byte[] inputBytes = Encoding.UTF8.GetBytes(text);
    
    using (MemoryStream outputStream = new MemoryStream())
    {
        using (GZipStream gzip = new GZipStream(outputStream, CompressionMode.Compress))
        {
            gzip.Write(inputBytes, 0, inputBytes.Length);
        }
        return outputStream.ToArray();
    }
}
```

##### Descompressão

```csharp
static string DecompressToString(byte[] compressedData)
{
    using (MemoryStream inputStream = new MemoryStream(compressedData))
    using (GZipStream gzip = new GZipStream(inputStream, CompressionMode.Decompress))
    using (MemoryStream outputStream = new MemoryStream())
    {
        gzip.CopyTo(outputStream);
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }
}
```

##### Uso Completo

```csharp
string input = "Texto para comprimir";

// Compressão
byte[] compressed = CompressString(input);
Console.WriteLine($"Comprimido (Base64): {Convert.ToBase64String(compressed)}");

// Descompressão
string decompressed = DecompressToString(compressed);
Console.WriteLine($"Original: {decompressed}");
```

##### Comparação de Tamanhos

| Texto Original | Tamanho Original | Tamanho Comprimido | Redução |
|---------------|------------------|-------------------|---------|
| 100 caracteres repetidos | 100 bytes | ~20 bytes | ~80% |
| JSON típico | 1 KB | ~200 bytes | ~80% |
| Texto aleatório | 1 KB | ~950 bytes | ~5% |

> A compressão é mais eficiente em dados com padrões repetitivos.

## Referências

- [GZipStream Class](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream)
- [BrotliStream](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.brotlistream)
- [Compression Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-compress-and-extract-files)
