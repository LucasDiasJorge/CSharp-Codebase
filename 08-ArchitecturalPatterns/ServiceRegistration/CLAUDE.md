# CLAUDE.md — ServiceRegistration

Web app sobre injeção de dependência e tempos de vida de serviço. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/ServiceRegistration/ServiceRegistration.csproj
dotnet run --project 08-ArchitecturalPatterns/ServiceRegistration/ServiceRegistration.csproj
```

## Estrutura interna

- `Services/Interface/` — `IMyService`, `IPreExecutionService`, `IPostExecutionService`.
- `Services/` — as implementações correspondentes.
- `Program.cs` — **o arquivo que importa**: é onde os tempos de vida (`Singleton`, `Scoped`, `Transient`) são escolhidos e onde a diferença entre eles se torna observável.

Os serviços pre/post existem para mostrar composição e ordem de execução no pipeline.

## Pontos de atenção

- TFM `net9.0` (a maioria da trilha está em `net8.0`), sem pacotes externos.
- Erro clássico que o exemplo ajuda a enxergar: injetar `Scoped` dentro de `Singleton` — o serviço com vida curta fica preso à instância longa. Ver também `03-WebAPIs/ShareableUser`.
