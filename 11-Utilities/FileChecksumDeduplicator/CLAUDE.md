# CLAUDE.md — FileChecksumDeduplicator

Console que calcula SHA-256 por stream, acha duplicatas filtrando por tamanho antes de ler conteúdo, e mede colisão e efeito de buffer. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/FileChecksumDeduplicator/FileChecksumDeduplicator.csproj
dotnet run -c Release --project 11-Utilities/FileChecksumDeduplicator/FileChecksumDeduplicator.csproj
```

**Usar `-c Release`**: quase todos os cenários medem tempo. Roda cinco cenários e termina. Sem serviço externo.

## Estrutura interna

`Hashing/FileHasher.ComputeStreaming` usa `IncrementalHash` + laço de `Read` com buffer explícito. O buffer é parâmetro porque o cenário 5 varia ele — trocar por `SHA256.HashData(stream)` simplifica o código e **elimina o cenário 5**.

`Hashing/DuplicateFinder` tem as duas estratégias. A diferença está em `ScanGroupingBySizeFirst`, que descarta grupos de tamanho com um só arquivo **antes** de abrir qualquer coisa. `FilesHashed` e `BytesRead` existem para o cenário 2 poder mostrar o custo evitado.

`Demo/SampleTreeFactory` monta a árvore com tamanhos deliberados: 300 arquivos de tamanhos **todos distintos** (nunca lidos pela estratégia esperta), 4 conteúdos × 3 cópias com tamanhos 180.000 a 180.003, um par de 64 KB que difere só no último byte, e um arquivo de 32 MB. **Mexer nesses tamanhos quebra os números do cenário 2** — se dois arquivos "únicos" passarem a ter o mesmo tamanho, eles entram na leitura.

Grava em `AppContext.BaseDirectory/dedup-demo` e apaga no fim de `RunAll`. Caminho relativo aqui espalharia 36 MB pela raiz do repositório, porque `dotnet run --project` mantém o diretório atual de quem chamou.

## Pontos de atenção

- TFM `net10.0`. Pacote: `Microsoft.Extensions.Logging.Console`.
- **O programa escreve ~36 MB em disco** a cada execução, incluindo um arquivo de 32 MB, e apaga no fim. Se for interrompido no meio, a pasta `dedup-demo` fica no diretório de saída da compilação.
- **Medição de memória já corrigida uma vez:** o cenário 1 media memória *retida* com `GC.GetTotalMemory` e imprimia `~0,0 MB` para as **duas** estratégias — o array de 32 MB já era inalcançável quando a medição rodava. Agora mede `GC.GetTotalAllocatedBytes(precise: true)`, que é a métrica que separa as duas. Não voltar para memória retida.
- **Cronômetro já corrigido uma vez:** `Environment.TickCount64` tem resolução de ~15ms, então o cenário 5 imprimia `0ms` e `Infinity MB/s` para o buffer de 1 MB. Tudo usa `Stopwatch` agora, e `HashResult.ElapsedMilliseconds` é `double`.
- **O cenário 5 aquece o cache** com uma leitura descartada antes de medir. Sem isso a primeira linha da tabela paga o disco frio e a comparação vira ruído. Os números são de arquivo em cache do SO — o README diz isso.
- O cenário 4 mede **uma** amostra de um processo aleatório: 385 contra 321 esperados, 103.802 contra 82.136. A variação é o próprio ponto, não um erro; o README explica. A busca de colisão é determinística (entradas `arquivo-N` em sequência), então os números se repetem entre execuções.
- `FindFirstCollision` tem teto de 50 milhões de tentativas. Aumentar `bits` além de 32 faz o cenário rodar por minutos ou estourar o teto e devolver `-1`.
- **Fronteira com os vizinhos**: `CsvStreamingProcessor` é o mesmo princípio sobre linhas de texto. `CompressDecompress` cobre streams de compressão. Não adicionar aqui remoção de arquivos duplicados — o exemplo localiza e explica; apagar é decisão de quem usa, e o README trata dos riscos (hard links, symlinks).
