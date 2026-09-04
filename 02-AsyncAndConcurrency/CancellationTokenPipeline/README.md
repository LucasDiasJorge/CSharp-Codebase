# CancellationTokenPipeline

Projeto console que demonstra a propagacao de `CancellationToken` entre etapas assincronas, linked tokens, encerramento cooperativo e o tratamento correto de `OperationCanceledException`.

## Visao geral

O exemplo modela um pipeline de documentos com tres etapas encadeadas — `Fetch`, `Transform` e `Publish` — e faz o mesmo token atravessar todas elas. Cada etapa representa um tipo diferente de trabalho: uma espera longa de I/O, um laco item a item e uma publicacao com limpeza obrigatoria no final.

Cinco cenarios rodam em sequencia: fluxo completo, cancelamento por timeout, cancelamento pelo usuario, encerramento cooperativo com resultado parcial e token ja cancelado antes de comecar. Ao executar, observe os timestamps dos logs: eles mostram exatamente em que etapa o cancelamento chegou e quanto trabalho foi concluido antes da parada.

O ponto central e que cancelamento em .NET e cooperativo: ninguem aborta uma `Task` de fora. O token so tem efeito onde o codigo o repassa e o verifica.

## Conceitos abordados

- Propagacao do `CancellationToken` como ultimo parametro de cada etapa assincrona.
- Linked tokens com `CancellationTokenSource.CreateLinkedTokenSource`, combinando cancelamento do usuario e deadline.
- Identificacao da origem real do cancelamento consultando cada `CancellationTokenSource`.
- `ThrowIfCancellationRequested` versus consulta a `IsCancellationRequested` para parada cooperativa.
- Tratamento de `OperationCanceledException` e `TaskCanceledException` com filtros de excecao.
- Limpeza nao cancelavel no `finally` usando `CancellationToken.None`.
- Callback de cancelamento com `CancellationToken.Register` e descarte da `CancellationTokenRegistration`.

## Objetivos de aprendizagem

- Entender por que uma espera que nao recebe o token continua ate o fim mesmo apos o cancelamento.
- Escolher entre lancar excecao e devolver resultado parcial conforme o valor do trabalho ja concluido.
- Descobrir qual origem disparou o cancelamento quando varios tokens estao ligados.
- Escrever compensacao e flush que completam mesmo depois de o token ser cancelado.
- Distinguir cancelamento esperado de falha real no bloco `catch`.

## Estrutura do projeto

```text
CancellationTokenPipeline/
|-- Demo/
|   `-- PipelineDemoRunner.cs
|-- Models/
|   |-- DocumentItem.cs
|   |-- PipelineResult.cs
|   `-- PipelineStatus.cs
|-- Pipeline/
|   |-- DocumentPipeline.cs
|   |-- FetchDocumentsStage.cs
|   |-- IPipelineStage.cs
|   |-- PublishDocumentsStage.cs
|   `-- TransformDocumentsStage.cs
|-- Services/
|   |-- CancellationReasonResolver.cs
|   `-- CooperativeBatchProcessor.cs
|-- CancellationTokenPipeline.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/CancellationTokenPipeline/CancellationTokenPipeline.csproj
```

Para validar compilacao:

```bash
dotnet build 02-AsyncAndConcurrency/CancellationTokenPipeline/CancellationTokenPipeline.csproj
```

A execucao completa leva cerca de 5 segundos e nao exige servico externo.

## Boas praticas e pontos de atencao

- Repasse o token para toda chamada assincrona interna. Um `Task.Delay` sem token ignora o cancelamento e segura o pipeline ate o fim da espera.
- Com linked tokens, a `OperationCanceledException` carrega o token *ligado*, nao o de origem. Comparar `excecao.CancellationToken` com o token do usuario nao funciona: pergunte a cada `CancellationTokenSource` se ela foi cancelada.
- `TaskCanceledException` e subclasse de `OperationCanceledException`; capturar a segunda cobre as duas.
- Limpeza, flush e compensacao devem usar `CancellationToken.None`, senao a propria limpeza e cancelada junto.
- Descarte a `CancellationTokenRegistration` devolvida por `Register`, ou o callback vive enquanto o `CancellationTokenSource` existir.
- `CancellationTokenSource` implementa `IDisposable`, principalmente quando usa `CancelAfter` (ha um timer por tras).
- Nao capture `OperationCanceledException` para transforma-la em sucesso silencioso: o chamador precisa saber que o trabalho ficou incompleto.
- O provider de console do `Microsoft.Extensions.Logging` grava em fila propria; por isso o runner aguarda um instante antes de imprimir o resumo de cada cenario, para que a ordem na tela reflita a ordem real.

## Conteudo complementar

Cenarios executados pelo `PipelineDemoRunner`:

| # | Cenario | Mecanismo demonstrado | Resultado esperado |
|---|---------|----------------------|--------------------|
| 1 | Fluxo completo | Token atravessa as etapas sem ser cancelado | 6 documentos publicados |
| 2 | Timeout | `CancellationTokenSource(TimeSpan)` ligado ao token do usuario | Parada em `Transform`, origem timeout |
| 3 | Cancelamento pelo usuario | `CancelAfter` no source do usuario, deadline folgado | Parada em `Publish`, origem usuario |
| 4 | Encerramento cooperativo | Laco consulta `IsCancellationRequested` e devolve parcial | 4 de 6 documentos, sem excecao |
| 5 | Token ja cancelado | `CancelAsync` antes da primeira etapa | Nenhum trabalho util, parada em `Fetch` |

Duas formas de reagir ao cancelamento dentro de um laco:

```text
ThrowIfCancellationRequested()  -> interrompe na hora, o chamador trata a excecao
IsCancellationRequested         -> termina o item corrente e devolve resultado parcial
```

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads
- https://learn.microsoft.com/dotnet/api/system.threading.cancellationtokensource
- https://learn.microsoft.com/dotnet/standard/parallel-programming/task-cancellation
