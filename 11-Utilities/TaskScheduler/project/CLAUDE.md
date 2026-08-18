# CLAUDE.md — TaskScheduler/project

Console que manipula o **Agendador de Tarefas do Windows**. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/TaskScheduler/project/project.csproj
dotnet run --project 11-Utilities/TaskScheduler/project/project.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): cria/consulta tarefas agendadas via a biblioteca `TaskScheduler` (wrapper da API do Windows) e registra eventos com `System.Diagnostics.EventLog`.

É a variante com log de eventos; `11-Utilities/TaskScheduler/production/` é a versão sem essa dependência.

## Pontos de atenção

- **TFM `net10.0-windows`** — só compila e roda no Windows.
- **Pode exigir privilégio elevado**: criar tarefa agendada e escrever no Event Log costuma falhar sem execução como administrador.
- Diretório e projeto se chamam `project`, fora do padrão PascalCase.
- **Sem README local** e **ausente do índice do README raiz** (a trilha 11 está listada com 10 projetos, sem contar este nem `production`).
