# CLAUDE.md — SerilogExample

Web API com logging estruturado Serilog, organizada em ports and adapters. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/SerilogExample/SerilogExample/SerilogExample.csproj
dotnet run --project 11-Utilities/SerilogExample/SerilogExample/SerilogExample.csproj
```

Requisições de exemplo em `SerilogExample.http`.

## Estrutura interna

Apesar do nome sugerir só logging, a arquitetura é o segundo tema:

- `Domain/Ports/IWeatherProvider.cs`, `IGetForecast.cs` — as portas.
- `Infrastructure/Adapters/Weather/InMemoryWeatherProvider.cs` — o adaptador.
- `Application/UseCases/GetForecast/GetForecastUseCase.cs` — o caso de uso.
- `Web/Controllers/ForecastController.cs` — entrada HTTP.
- `Program.cs` — configuração do Serilog com sinks de console e arquivo.

Logging **estruturado** significa usar template com propriedades nomeadas (`"Pedido {OrderId} processado"`), não interpolação de string. A interpolação destrói a estrutura e é o erro a evitar ao editar.

## Pontos de atenção

- TFM **`net10.0`**. `Microsoft.AspNetCore.OpenApi` está numa versão **release candidate** (`10.0.0-rc.2.25502.107`) — se houver falha de restore ou incompatibilidade, é o suspeito principal.
- O sink de arquivo grava logs em disco na execução; não comite os artefatos.
- **Sem README local**; documentação em `11-Utilities/SerilogExample/`. O diretório-pai tem o mesmo nome do projeto — atenção ao caminho duplicado nos comandos.
