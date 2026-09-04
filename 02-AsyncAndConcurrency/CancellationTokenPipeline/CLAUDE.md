# CLAUDE.md — CancellationTokenPipeline

Console que demonstra propagação de `CancellationToken` entre etapas assíncronas, linked tokens e encerramento cooperativo. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/CancellationTokenPipeline/CancellationTokenPipeline.csproj
dotnet run --project 02-AsyncAndConcurrency/CancellationTokenPipeline/CancellationTokenPipeline.csproj
```

## Estrutura interna

Pipeline de três etapas (`Pipeline/`), todas implementando `IPipelineStage<TInput, TOutput>` e recebendo o token como último parâmetro:

- `FetchDocumentsStage` — espera única de I/O; repassa o token para `Task.Delay` (lança `TaskCanceledException`).
- `TransformDocumentsStage` — laço item a item com `ThrowIfCancellationRequested`.
- `PublishDocumentsStage` — `CancellationToken.Register` para callback e flush no `finally` com `CancellationToken.None`.

`DocumentPipeline` orquestra e **não engole** a exceção: expõe `CurrentStageName` e `LoadedDocuments` para o chamador montar o relatório parcial. `Services/CancellationReasonResolver` descobre a origem do cancelamento consultando cada `CancellationTokenSource` — com linked token a exceção carrega o token ligado, não o de origem. `Services/CooperativeBatchProcessor` é o contraponto: consulta `IsCancellationRequested` e devolve `PipelineResult` parcial em vez de lançar.

`Demo/PipelineDemoRunner` executa os 5 cenários; `Program.cs` monta o `ILoggerFactory` com `AddSimpleConsole` (linha única + timestamp).

## Pontos de atenção

- TFM `net10.0` (não `net9.0` como a maioria da trilha): a máquina não tem runtime 9.0 instalado, e este sample precisa ser executado, não só compilado. Não "padronizar" para net9.0 sem verificar os runtimes disponíveis.
- Única dependência: `Microsoft.Extensions.Logging.Console` 10.0.11. Sem serviço externo.
- Os tempos dos cenários são calibrados (`FetchLatency` 300ms, `ItemLatency` 120ms, timeout 700ms, cancelamento do usuário em 1300ms) para que cada cancelamento caia numa etapa diferente. Alterar uma constante muda em qual etapa o pipeline para e invalida a tabela de cenários do README.
- `PrintResultAsync` aguarda `LogFlushDelay` (100ms) de propósito: o provider de console loga em thread própria e, sem a pausa, o resumo `>>` aparece antes dos logs que ele resume.
