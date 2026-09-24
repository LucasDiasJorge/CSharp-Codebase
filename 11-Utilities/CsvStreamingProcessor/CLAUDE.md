# CLAUDE.md — CsvStreamingProcessor

Console que lê CSV por stream, com parser que respeita aspas, erros por linha e escrita incremental. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/CsvStreamingProcessor/CsvStreamingProcessor.csproj
dotnet run -c Release --project 11-Utilities/CsvStreamingProcessor/CsvStreamingProcessor.csproj
```

**Usar `-c Release`**: os cenários 1 e 5 medem tempo e memória. Roda cinco cenários e termina. Sem serviço externo.

## Estrutura interna

`Csv/StreamingCsvReader.ReadRows` devolve `IEnumerable<CsvRow>` com `yield return`. **Essa assinatura é o exemplo inteiro** — trocar para `List<CsvRow>` materializa o arquivo e o cenário 1 passa a medir a mesma coisa duas vezes.

`ParseLine` é público de propósito: o cenário 1 usa o mesmo parser nas duas estratégias, para que a comparação isole a memória e não o trabalho feito. Tornar privado de novo quebra o cenário.

`CsvRow` é registro **ou** erro, nunca exceção. É o que permite continuar após uma linha ruim.

`Csv/CsvLineParser` trata aspas e `""` escapado. O cenário 4 compara com `Split(';')`.

`Demo/SampleFileFactory` grava em `AppContext.BaseDirectory/csv-demo` e apaga no fim de `RunAll`. **Caminho relativo aqui espalharia arquivos pela raiz do repositório**, porque `dotnet run --project` mantém o diretório atual de quem chamou — foi exatamente o que aconteceu nos demos de SQLite de `09-Data` (commit `4ae55d3`).

## Pontos de atenção

- TFM `net10.0`. Pacote: `Microsoft.Extensions.Logging.Console`.
- **O cenário 1 gera 200 mil linhas (~8,5 MB) e retém ~51 MB** na estratégia comparativa. Aumentar `LargeFileRows` multiplica as duas coisas.
- **Medição de memória já corrigida uma vez:** a primeira versão media `ReadAllLines` antes do streaming e imprimia `-22,6 MB retidos` — negativo, porque o array anterior só foi coletado depois da linha de base seguinte. A correção foi medir o streaming primeiro, com `Collect()` (dois `GC.Collect` com `WaitForPendingFinalizers` entre eles) antes de cada linha de base. Ao mexer na ordem dos cenários, manter isso.
- **A comparação do cenário 1 também já foi injusta:** o streaming fazia o parsing completo e o `ReadAllLines` só carregava texto, então o streaming aparecia como mais lento (180ms contra 43ms). Agora as duas estratégias produzem `SaleRecord` com o mesmo parser. Não "simplificar" o lado do `ReadAllLines` de volta.
- `FormatMegabytes` imprime `~0,0` abaixo de 0,1 MB. Isso não é maquiagem: o valor exato está abaixo da resolução da medida, e `-0,00 MB` só confunde.
- **O cenário 2 depende de `129,90` ser aceito pelas duas culturas com valores diferentes** (129,90 contra 12990). É o achado central do exemplo. Trocar `NumberStyles.Number` por `NumberStyles.Float` faz a invariante rejeitar a vírgula e o cenário perde a graça.
- Os tempos (293ms / 160ms / 96ms) variam entre execuções. O README cita a ordem de grandeza e a razão entre eles, não o número como constante.
- **Fronteira com os vizinhos**: `Serialization` e `ClassToXml` cobrem conversão objeto ↔ formato. Aqui o assunto é o fluxo. Não adicionar `CsvHelper` — o parser à mão existe para mostrar o problema que a biblioteca resolve; o README já recomenda a biblioteca para uso real.
