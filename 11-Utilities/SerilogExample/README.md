# SerilogExample (Ports and Adapters)

API mínima organizada em portas e adaptadores (hexagonal). Serilog fica para você plugar depois; aqui há um guia rápido de logging estruturado.

## Arquitetura
- `Domain`: modelo e contratos de portas (`IGetForecast` inbound, `IWeatherProvider` outbound).
- `Application`: caso de uso orquestra o domínio (`GetForecastUseCase`).
- `Infrastructure`: adaptador que satisfaz portas de saída (`InMemoryWeatherProvider`).
- `Web`: adaptador primário (endpoints HTTP) em `Web/Endpoints/WeatherEndpoints.cs`.

Fluxo do `/api/forecast`:
1) Endpoint recebe `days` (default 5).
2) Usa a porta inbound `IGetForecast`.
3) Caso de uso pede dados à porta outbound `IWeatherProvider`.
4) Adaptador in-memory devolve a coleção de `WeatherReading` (domínio puro).

## Como rodar
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\SerilogExample\SerilogExample"
dotnet run
# GET https://localhost:5001/api/forecast?days=3
```

## Onde plugar o Serilog depois
1) Pacotes sugeridos:
   - `Serilog.AspNetCore`
   - `Serilog.Enrichers.Environment`
   - `Serilog.Sinks.Console`
   - (opcional) `Serilog.Sinks.File`, `Serilog.Sinks.Seq`
2) Em `Program.cs`, antes de `builder.Build()`:
```csharp
// using Serilog;
// builder.Host.UseSerilog((ctx, services, cfg) => cfg
//     .ReadFrom.Configuration(ctx.Configuration)
//     .ReadFrom.Services(services)
//     .Enrich.FromLogContext()
//     .Enrich.WithEnvironmentName()
//     .WriteTo.Console());
```
3) No `appsettings*.json`, adicione a seção `Serilog` para controlar nível, enrichers e sinks.
4) Dentro das camadas, injete `ILogger<T>` (já fornecido pelo host) e use propriedades nomeadas.

## Dicas práticas de logging estruturado
- Prefira `LogInformation("Processando forecast para {Days}", days);` em vez de interpolar strings.
- Sempre inclua correlação: `TraceIdentifier`, `RequestId`, `UserId` (quando houver).
- Níveis típicos: `Debug` (detalhe de desenvolvimento), `Information` (eventos de negócio), `Warning` (situações inesperadas), `Error` (falhou, mas continuou), `Fatal` (serviço indisponível).
- Mantenha mensagens curtas e invariantes; propriedades carregam o contexto.
- Não logue dados sensíveis (tokens, senhas, PII). Mas logue chaves anônimas (hashes, ids).

### Exemplos de eventos úteis
- Início/fim de caso de uso: `LogInformation("GetForecastStarted", new { Days = days });`
- Integrações externas: `LogInformation("WeatherProviderCall", new { Days = days, Provider = "InMemory" });`
- Erros com contexto: `LogError(ex, "WeatherProviderFailed", new { Days = days });`
- Métrica derivada: logue contadores ou tempos (`ElapsedMs`) para alimentar dashboards.

### Estrutura sugerida de propriedades
- `event` (nome curto do evento)
- `message` (frase humana, sem interpolar dados)
- `user_id` / `tenant_id`
- `request_id` / `trace_id` / `span_id`
- `context` (objeto com dados do domínio)
- `elapsed_ms` (quando medir duração)

## Teste rápido do endpoint
```powershell
# padrão (5 dias)
Invoke-RestMethod -Uri "https://localhost:5001/api/forecast" -SkipCertificateCheck

# escolhendo dias
Invoke-RestMethod -Uri "https://localhost:5001/api/forecast?days=10" -SkipCertificateCheck
```

## Próximos passos sugeridos
- Trocar `InMemoryWeatherProvider` por um adaptador real (HTTP, banco, cache).
- Adicionar testes de unidade para o caso de uso e para o adaptador.
- Configurar Serilog com console JSON e (opcional) um sink de observabilidade.

_[reference](https://medium.com/@iamprovidence/step-by-step-guide-to-logs-in-c-best-practices-and-tips-491c8d555d26)_