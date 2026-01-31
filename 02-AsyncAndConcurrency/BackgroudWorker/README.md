# BackgroudWorker (Hosted Service)

## Visão Geral
Exemplo de serviço em background usando timer periódico para demonstrar ciclo de vida (`StartAsync` / execução recorrente / `StopAsync`). Foca em estrutura simples e clara.

## Objetivos Didáticos
- Implementar `IHostedService` (ou `BackgroundService`).
- Usar `Timer` para execução periódica.
- Demonstrar logging e cancellation token.

## Estrutura
```
BackgroudWorker/
  Program.cs
  TimedHostedService.cs
```
`TimedHostedService` registra um timer que executa a ação em intervalo fixo.

## Tecnologias
| Categoria | Recurso | Observação |
|-----------|---------|-----------|
| Hosting | IHostedService | Integração com pipeline do .NET |
| Timer | System.Threading.Timer | Execução periódica |
| Logging | ILogger<TimedHostedService> | Logs estruturados |

## Como Executar
```powershell
dotnet restore
dotnet run --project ./BackgroudWorker/BackgroudWorker.csproj
```

## Fluxo Principal
1. Aplicação inicia e registra o hosted service.
2. `StartAsync` agenda o timer imediato e intervalado.
3. Cada tick executa `DoWork` (lógica simulada / placeholder).
4. Shutdown chama `Dispose`/`StopAsync` liberando recursos.

## Boas Práticas Demonstradas
- Uso explícito de tipos (sem `var`).
- Logging em cada fase do ciclo de vida.
- Respeito a cancellation tokens.

## Pontos de Atenção
- Timer simples (não resiliente a long running work). Para workloads longos, preferir fila + loop async.
- Exemplo não implementa política de retry (pode ser extensão futura).

## Próximos Passos Sugeridos
- Implementar backoff exponencial em falhas.
- Trocar `Timer` por `PeriodicTimer` (quando adequado).
- Adicionar health check customizado do worker.

---
README adaptado ao template comum.
