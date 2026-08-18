# CLAUDE.md — BlazorHelloWorld

Aplicação Blazor introdutória. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/BlazorHelloWorld/BlazorHelloWorld.csproj
dotnet run --project 03-WebAPIs/BlazorHelloWorld/BlazorHelloWorld.csproj
```

## Estrutura interna

Estrutura padrão do template Blazor, praticamente intocada:

- `Components/App.razor` e `Components/Routes.razor` — raiz e roteamento.
- `Components/Layout/` — `MainLayout`, `NavMenu`, `ReconnectModal` (este último indica **render mode interativo por servidor**: a reconexão só existe no circuito SignalR).
- `Components/Pages/` — `HelloWorld` é a página didática do sample; `Counter`, `Weather`, `Home`, `Error`, `NotFound` vêm do template.

Quase toda a lógica vive em `.razor`, não em `.cs` — `Program.cs` só registra os serviços e mapeia os componentes.

## Pontos de atenção

- TFM **`net10.0`** (a maioria da trilha está em `net9.0`).
- Ao editar `.razor`, a regra de não usar `var` do repositório continua valendo dentro dos blocos `@code`.
