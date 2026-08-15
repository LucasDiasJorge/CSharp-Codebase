# CLAUDE.md — CompressDecompress

Console: compressão e descompressão de strings com `GZipStream`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/CompressDecompress/CompressDecompress.csproj
dotnet run --project 11-Utilities/CompressDecompress/CompressDecompress.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): texto → bytes → `GZipStream` → Base64 e o caminho inverso, com as taxas de compressão impressas.

Detalhe que costuma quebrar implementações reais e que o exemplo respeita: o `GZipStream` precisa ser **fechado/liberado antes** de ler o buffer de saída, senão o flush final não acontece e o resultado sai truncado.

## Pontos de atenção

- TFM `net9.0`, sem pacotes externos (`System.IO.Compression` está no runtime).
- Texto curto pode **crescer** depois de comprimido — o cabeçalho gzip tem custo fixo. Não é bug; é útil deixar visível na saída.
