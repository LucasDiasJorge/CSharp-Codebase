# CsvStreamingProcessor

Console que processa arquivos CSV grandes sem carregá-los por inteiro, tratando cultura, campos entre aspas, registros inválidos e escrita incremental.

## Visão geral

Ler um CSV com `File.ReadAllLines` funciona até o dia em que o arquivo cresce. O exemplo mede a diferença: 200 mil linhas, **8,5 MB em disco**, viram **51,3 MB retidos** quando as linhas e os registros ficam todos vivos — e **praticamente zero** quando o arquivo é percorrido por stream. A versão em streaming aloca as mesmas strings; o que ela não faz é **segurá-las**.

O que sustenta isso é o `yield return`: o método devolve o controle a cada linha, e o `StreamReader` só avança quando o consumidor pede a próxima. Trocar o retorno por `List<CsvRow>` materializa tudo e desfaz a propriedade inteira.

Os outros quatro cenários tratam do que costuma quebrar em importação de arquivo real. **Cultura** é o mais perigoso, porque falha em silêncio: `"129,90"` lido com `InvariantCulture` vira **12990**, não 129,90 — as duas culturas aceitam o texto e discordam do valor, sem exceção nenhuma. **Linhas inválidas** não podem derrubar o lote: o leitor devolve registro ou erro com o número da linha, e a importação continua. **Aspas** quebram o `Split(';')` no primeiro campo que contenha o separador. E o **pipeline incremental** lê, filtra e escreve mantendo a memória plana dos dois lados.

## Conceitos abordados

- `yield return` e avaliação preguiçosa sobre `StreamReader`.
- Memória alocada contra memória retida.
- `CultureInfo` na conversão de números e datas.
- Falha silenciosa por cultura: as duas aceitam, com valores diferentes.
- `TryParse` com `NumberStyles`/`DateTimeStyles` explícitos.
- Erro como valor de retorno, não como exceção, para não abortar o lote.
- Campo entre aspas com separador interno e aspas escapadas.
- Escrita incremental com `StreamWriter`.

## Objetivos de aprendizagem

- Escrever leitura de arquivo que não cresça em memória com o tamanho da entrada.
- Distinguir o que é alocado do que fica retido, e medir os dois.
- Escolher a cultura de conversão de forma consciente, em vez de herdar a do sistema.
- Relatar linhas inválidas de forma corrigível, com número de linha e motivo.
- Reconhecer quando `Split` não basta para CSV.

## Estrutura do projeto

```text
CsvStreamingProcessor/
|-- Csv/
|   |-- CsvLineParser.cs
|   |-- SaleRecord.cs
|   `-- StreamingCsvReader.cs
|-- Demo/
|   |-- CsvDemoRunner.cs
|   `-- SampleFileFactory.cs
|-- CsvStreamingProcessor.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run -c Release --project 11-Utilities/CsvStreamingProcessor/CsvStreamingProcessor.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 11-Utilities/CsvStreamingProcessor/CsvStreamingProcessor.csproj
```

Não exige serviço externo. O programa **gera** os arquivos de teste (cerca de 9 MB) em uma pasta `csv-demo` dentro do diretório de saída da compilação e **apaga tudo ao final**. Use `-c Release`, porque os cenários 1 e 5 medem tempo e memória.

## Boas práticas e pontos de atenção

- Prefira `IEnumerable<T>` com `yield return` a devolver `List<T>` em leitura de arquivo. Quem consome decide se quer materializar; devolvendo lista, a decisão já foi tomada por quem menos sabe do tamanho da entrada.
- **Sempre passe a cultura explicitamente.** `decimal.Parse(texto)` usa a cultura da thread, que muda com a máquina, com o contêiner e com o `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT`. O mesmo arquivo passa a ser lido de formas diferentes em ambientes diferentes.
- Para arquivos de intercâmbio entre sistemas, `InvariantCulture` com formato `yyyy-MM-dd` é a escolha defensável. Para arquivos exportados por humanos com Excel em português, a cultura do arquivo é `pt-BR` — e isso precisa ser uma decisão registrada, não um acidente.
- Use `TryParse`, não `Parse`, em dado externo. Dado de arquivo é entrada não confiável por definição.
- Devolva o erro como valor, com número de linha e motivo. Exceção por linha inválida transforma um arquivo com 3 defeitos em 3 reprocessamentos completos.
- Nunca use `Split` para CSV de produção. O separador dentro de aspas é comum em endereço, descrição e nome de produto; o resultado é um campo a mais e **todos** os seguintes deslocados.
- Para CSV de verdade, considere `CsvHelper` ou `Sep`. O parser daqui existe para mostrar o problema, não para substituir uma biblioteca testada contra o RFC 4180.
- Escreva a saída de forma incremental. Acumular resultado em `List<T>` para gravar no fim recria o problema de memória do lado da escrita.
- `File.ReadLines` (sem o `All`) já é preguiçoso e é a forma mais curta de conseguir o mesmo efeito quando não há parsing próprio envolvido.

## Conteúdo complementar

**1. Carregar tudo contra ler por stream** — 200.000 linhas, 8,5 MB em disco, mesmo parser nos dois lados:

| Estratégia | Registros | Memória retida | Tempo |
|---|---:|---:|---:|
| `ReadAllLines` + `List<SaleRecord>` | 200.000 | **51,3 MB** | 293ms |
| Streaming com `yield` | 200.000 | **~0 MB** | 160ms |

O arquivo tem 8,5 MB e a versão que retém tudo ocupa 51,3 MB: cada linha vira um `string` com overhead de objeto, mais um registro com suas próprias referências. O streaming é também **mais rápido**, porque não paga alocação de array grande nem pressão de GC.

**2. Cultura, onde a corrupção é silenciosa**:

| Texto | `pt-BR` | `InvariantCulture` | |
|---|---|---|---|
| `129,90` | 129,90 | **12990** | os dois aceitaram, valores diferentes |
| `1.234` | 1234 | **1,234** | os dois aceitaram, valores diferentes |
| `1.234,56` | 1234,56 | erro | |
| `1,234.56` | erro | 1234,56 | |
| `2026-01-15` | 2026-01-15 | 2026-01-15 | ISO funciona nas duas |
| `15/01/2026` | 2026-01-15 | erro | |
| `01/02/2026` | 2026-02-01 | **2026-01-02** | dia e mês trocados, sem erro |

As linhas perigosas não são as que dão erro — são as que **as duas culturas aceitam com valores diferentes**. `"129,90"` lido como invariante vira 12990 porque a vírgula é tratada como separador de milhar. Um relatório com 100x o valor correto e nenhum log.

**3. Linhas inválidas** — arquivo de 7 registros:

```text
4 registros validos, 3 rejeitados:
  linha 4: quantidade invalida: 'abc'
  linha 5: esperados 6 campos, encontrados 5
  linha 6: quantidade precisa ser positiva, veio 0
```

**4. Aspas, onde o `Split` quebra**:

```text
linha: 6;"Cabo HDMI; 2m";3;39,90;2026-01-20;Sudeste
  Split(';'): 7 campos -> produto = ""Cabo HDMI"
  parser:     6 campos -> produto = "Cabo HDMI; 2m"
```

Sete campos onde havia seis: o produto foi cortado ao meio e **todos os campos seguintes deslocaram**, então a quantidade passa a ser lida da coluna do valor. A segunda linha do cenário é mais sutil — o campo tem aspa escapada mas não tem separador interno, então o `Split` acerta a **contagem** e ainda assim entrega o valor errado, com as aspas cruas no meio do texto.

**5. Pipeline incremental** — filtrar a região Sul das 200 mil linhas:

```text
39.966 linhas escritas em 96ms (0,9 MB de saida)
total faturado: R$ 189.064.585,29
memoria retida pelo pipeline inteiro: ~0,0 MB
```

Entrada de 200 mil linhas, saída de quase 40 mil, e a memória não acompanha nenhuma das duas — é isso que permite processar arquivo maior que a RAM disponível.

Relação com os vizinhos: `Serialization` e `ClassToXml` tratam de converter objetos para formatos estruturados; aqui o assunto é o **fluxo** de leitura e escrita. `FileChecksumDeduplicator` aplica a mesma ideia de streaming a bytes em vez de linhas.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/csharp/language-reference/statements/yield
- https://learn.microsoft.com/dotnet/api/system.io.file.readlines
- https://learn.microsoft.com/dotnet/standard/globalization-localization/globalization
- https://datatracker.ietf.org/doc/html/rfc4180
