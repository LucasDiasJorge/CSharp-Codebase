# BackgroudWorker (Hosted Service)

## Visão geral

Exemplo de serviço em background usando timer periódico para demonstrar ciclo de vida (`StartAsync` / execução recorrente / `StopAsync`). Foca em estrutura simples e clara.

## Conceitos abordados

- Exemplo didático sobre BackgroudWorker (Hosted Service) no contexto de assincronia, tasks, threads e coordenação de trabalho.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Implementar `IHostedService` (ou `BackgroundService`).
- Usar `Timer` para execução periódica.
- Demonstrar logging e cancellation token.

## Estrutura do projeto

```text
BackgroudWorker/
+-- BackgroudWorker/
+-- Properties/
|   \-- launchSettings.json
+-- appsettings.Development.json
+-- appsettings.json
+-- BackgroudWorker.csproj
+-- BackgroudWorker.csproj.user
+-- BackgroudWorker.http
+-- Program.cs
\-- ...
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/BackgroudWorker/BackgroudWorker.csproj
```

## Boas práticas e pontos de atenção

- Uso explícito de tipos (sem `var`).
- Logging em cada fase do ciclo de vida.
- Respeito a cancellation tokens.

### Pontos de Atenção

- Timer simples (não resiliente a long running work). Para workloads longos, preferir fila + loop async.
- Exemplo não implementa política de retry (pode ser extensão futura).

## Conteúdo complementar

##### Estrutura

```
BackgroudWorker/
  Program.cs
  TimedHostedService.cs
```
`TimedHostedService` registra um timer que executa a ação em intervalo fixo.

##### Tecnologias

| Categoria | Recurso | Observação |
|-----------|---------|-----------|
| Hosting | IHostedService | Integração com pipeline do .NET |
| Timer | System.Threading.Timer | Execução periódica |
| Logging | ILogger<TimedHostedService> | Logs estruturados |

##### Fluxo Principal

1. Aplicação inicia e registra o hosted service.
2. `StartAsync` agenda o timer imediato e intervalado.
3. Cada tick executa `DoWork` (lógica simulada / placeholder).
4. Shutdown chama `Dispose`/`StopAsync` liberando recursos.

##### Próximos Passos Sugeridos

- Implementar backoff exponencial em falhas.
- Trocar `Timer` por `PeriodicTimer` (quando adequado).
- Adicionar health check customizado do worker.
