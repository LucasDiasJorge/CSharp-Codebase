# CLAUDE.md — TaskScheduler/production

Console que manipula o Agendador de Tarefas do Windows, sem Event Log. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/TaskScheduler/production/production.csproj
dotnet run --project 11-Utilities/TaskScheduler/production/production.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): agendamento de tarefas via a biblioteca `TaskScheduler`. Depende **apenas** desse pacote — é a diferença em relação a `11-Utilities/TaskScheduler/project/`, que também usa `System.Diagnostics.EventLog`.

## Pontos de atenção

- **TFM `net10.0-windows`** — só compila e roda no Windows.
- **Pode exigir privilégio elevado** para criar ou alterar tarefas agendadas.
- Diretório e projeto se chamam `production`, fora do padrão PascalCase; apesar do nome, é material didático.
- **Sem README local** e ausente do índice do README raiz.
