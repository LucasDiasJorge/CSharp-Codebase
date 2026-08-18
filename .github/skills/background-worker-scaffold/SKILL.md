---
name: background-worker-scaffold
description: "Use para criar a base de um worker de background (BackgroundService/IHostedService) em qualquer projeto .NET do workspace, SEM logica de fila (sem QueueService, ConsumeQueue ou consumer.Execute), pronta para plugar logica propria. Herda a classe base de worker do projeto quando existir, senao BackgroundService. Antes de gerar, a skill pergunta o modelo de paralelismo e a cadencia. Foca em DI correta (hosted service singleton + dependencias scoped via IServiceProvider.CreateScope), escalabilidade e cancelamento gracioso. Use ao criar um novo worker de processamento em segundo plano."
argument-hint: "Informe o nome do worker, o projeto/namespace alvo, as dependencias a injetar (e se sao scoped) e o grau de paralelismo desejado."
user-invocable: true
---

# Background Worker Scaffold

## Objetivo
Gerar a base de um worker de background que herda a classe base de worker do projeto ou `BackgroundService`, pronta para receber logica propria, sem acoplar a consumo de fila. O esqueleto ja resolve os dois pontos criticos: injecao de dependencia correta para um hosted service e escalabilidade por paralelismo controlado com desligamento gracioso.

## Quando usar
- Criar um novo worker de processamento em segundo plano em qualquer projeto .NET do workspace.
- Precisar de um `BackgroundService` para plugar logica de polling, agendamento ou processamento continuo.
- Reaproveitar a classe base de worker do projeto sem a parte de fila (`QueueService.ConsumeQueue` / `consumer.Execute`).

## Nao usar quando
- O worker for consumir uma fila existente: siga o padrao com `QueueService`/`IConsumer`, nao esta skill.
- A intencao for logica sincrona de request/response: use um service comum, nao um worker.

## Entradas minimas
- Nome do worker (ex.: `AssetReconciliationWorker`).
- Projeto/namespace alvo.
- Classe base de worker do projeto, se existir; caso contrario o esqueleto herda `BackgroundService`.
- Dependencias que a logica vai usar e seus lifetimes (`scoped` vs `singleton`).
- Grau de paralelismo e intervalo entre ciclos.

## Perguntas obrigatorias antes de gerar
Nao assuma; pergunte ao usuario e so entao gere o esqueleto:
1. Modelo de paralelismo: (a) N loops paralelos com `Task.WhenAll`; (b) loop unico; (c) lote por ciclo com `Parallel.ForEachAsync` e `MaxDegreeOfParallelism`.
2. Cadencia: (a) polling continuo com `Task.Delay`; (b) intervalo fixo com `PeriodicTimer`; (c) rodar uma vez e encerrar.
3. Classe base a herdar: base de worker do projeto ou `BackgroundService`.
4. Dependencias e lifetimes, para decidir o que injetar no construtor e o que resolver por escopo.

Se o usuario nao decidir, use o default recomendado (N loops paralelos + polling continuo + base do projeto) e registre como premissa explicita.

## Padrao de referencia
- Se o projeto tiver uma classe base abstrata de worker (tipicamente `abstract : BackgroundService`, com logger e helper de erro ja expostos), herde-a e reaproveite o tratador de excecao dela; senao herde `BackgroundService` e injete `ILogger<T>` diretamente.
- Workers concretos usam construtor primario encadeando a base.
- Hosted services sao singleton, registrados com `services.AddHostedService<T>()` na configuracao de DI do projeto. Dependencia `scoped` nunca vai no construtor do worker.
- Resolucao de `scoped` dentro de um singleton usa `IServiceProvider.CreateScope()` por unidade de trabalho.
- Desligamento gracioso respeita o `CancellationToken` em cada `await`.

## Procedimento
1. Coletar as entradas e responder as "Perguntas obrigatorias".
2. Criar a classe em `Business/Workers/` (ou pasta equivalente) herdando a base escolhida, com construtor primario.
3. Injetar apenas singletons no construtor: `ILogger<T>`, `IServiceProvider` (e os singletons exigidos pela base do projeto, quando houver). Nunca injetar `scoped` direto.
4. Definir a constante de paralelismo e/ou o intervalo conforme as escolhas.
5. Implementar `ExecuteAsync` com a variante de paralelismo escolhida.
6. Aplicar a variante de cadencia ao redor de `ProcessAsync`.
7. Em cada ciclo, abrir `CreateScope()` e resolver ali as dependencias `scoped`; deixar `ProcessAsync` como ponto de extensao.
8. Tratar cancelamento e excecao: o cancelamento encerra o loop; demais excecoes vao para o tratador sem derrubar o worker.
9. Registrar com `AddHostedService<T>()` e garantir os `scoped` registrados na DI do projeto.
10. Rodar `dotnet build` e confirmar que o host sobe sem erro de captured dependency.

## Esqueleto canonico
Ponto de extensao unico `ProcessAsync`; os blocos de paralelismo e cadencia sao trocados conforme as escolhas. Ao herdar a base de worker do projeto, troque `BackgroundService` por ela encadeando os parametros exigidos, remova o campo `_logger` se a base ja o expuser e use o tratador de erro da base como `HandleFailure`.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Company.Project.Business.Workers;

/// <summary>Base background worker; plug domain logic into ProcessAsync.</summary>
public class SampleBackgroundWorker(
    ILogger<SampleBackgroundWorker> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<SampleBackgroundWorker> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    // [PARALELISMO] ExecuteAsync entra aqui.

    // Resolve scoped dependencies per cycle; the hosted service itself is a singleton.
    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        // IMyService service = scope.ServiceProvider.GetRequiredService<IMyService>();
        // await service.RunAsync(cancellationToken);
        await Task.CompletedTask;
    }

    private void HandleFailure(Exception ex)
        => _logger.LogError(ex, "[Worker:Sample] Cycle failed.");
}
```

### Paralelismo (escolha 1)
(a) N loops paralelos:
```csharp
public const int NUMBER_OF_WORKERS = 3;

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    Task[] loops = new Task[NUMBER_OF_WORKERS];
    for (int i = 0; i < NUMBER_OF_WORKERS; i++)
        loops[i] = RunLoopAsync(i, stoppingToken);

    await Task.WhenAll(loops);
}
```
(b) Loop unico:
```csharp
protected override Task ExecuteAsync(CancellationToken stoppingToken)
    => RunLoopAsync(0, stoppingToken);
```
(c) Lote por ciclo (dentro de `ProcessAsync`, sobre itens resolvidos no escopo):
```csharp
ParallelOptions options = new() { MaxDegreeOfParallelism = 4, CancellationToken = cancellationToken };
await Parallel.ForEachAsync(batch, options, async (item, ct) => await handler.HandleAsync(item, ct));
```

### Cadencia (escolha 2) — corpo de `RunLoopAsync`
(a) Polling continuo:
```csharp
private async Task RunLoopAsync(int index, CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        try { await ProcessAsync(cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { break; }
        catch (Exception ex) { HandleFailure(ex); }

        try { await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); }
        catch (TaskCanceledException) { break; }
    }
}
```
(b) `PeriodicTimer` (intervalo fixo, sem drift):
```csharp
private async Task RunLoopAsync(int index, CancellationToken cancellationToken)
{
    using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));
    while (await timer.WaitForNextTickAsync(cancellationToken))
    {
        try { await ProcessAsync(cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { break; }
        catch (Exception ex) { HandleFailure(ex); }
    }
}
```
(c) Rodar uma vez e encerrar (substitui `ExecuteAsync`):
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    try { await ProcessAsync(stoppingToken); }
    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    catch (Exception ex) { HandleFailure(ex); }
}
```

Registro na configuracao de DI do projeto:
```csharp
services.AddHostedService<SampleBackgroundWorker>();
// Registre as dependencias scoped usadas em ProcessAsync.
```

## DI e escalabilidade (regras obrigatorias)
- O hosted service e singleton: no construtor apenas singletons (`ILogger<T>`, `IServiceProvider`/`IServiceScopeFactory` e os singletons exigidos pela base do projeto, quando houver).
- Toda dependencia `scoped` (repositorio, service de dominio, `IDbConnection`) e resolvida dentro de `CreateScope()` por ciclo; nunca capturada em campo do worker.
- Descarte o escopo ao fim de cada ciclo (`using`) para nao vazar conexoes.
- Ajuste a escala por `NUMBER_OF_WORKERS` ou `MaxDegreeOfParallelism`; cada loop tem seu proprio escopo e nao compartilha estado mutavel com os demais.
- Respeite o `CancellationToken` em todo `await` (loop, `Task.Delay` e `PeriodicTimer`); o desligamento deve ser gracioso, sem `Thread.Sleep` nem espera bloqueante.
- Prefira APIs assincronas; nao use `.Result`/`.Wait()` dentro do loop.
- Nao registrar segredos, credenciais ou payload sensivel em log; use templates estruturados `[Worker:Nome]`.

## Checklist de conclusao
- [ ] Worker herda a base escolhida (base do projeto ou `BackgroundService`) com construtor primario.
- [ ] Somente singletons no construtor; `scoped` via `CreateScope()`.
- [ ] Variante de paralelismo e de cadencia aplicadas conforme decidido.
- [ ] Cancelamento gracioso coberto.
- [ ] Falha de ciclo tratada sem derrubar o worker.
- [ ] `AddHostedService<T>()` adicionado e dependencias `scoped` registradas.
- [ ] `dotnet build` passa e o host inicia sem erro de captured dependency.

## Anti-padroes
- Injetar repositorio/service `scoped` direto no worker (captured dependency).
- Reintroduzir `QueueService`/`ConsumeQueue`/`consumer.Execute` quando o objetivo e uma base sem fila.
- Busy loop sem `Task.Delay`/`PeriodicTimer` ou sem checagem de cancelamento.
- Engolir excecao sem log/alerta ou, no oposto, deixar uma falha de ciclo encerrar o worker.
