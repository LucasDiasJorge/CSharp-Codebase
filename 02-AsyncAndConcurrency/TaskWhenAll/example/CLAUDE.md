# CLAUDE.md — TaskWhenAll/example

Console focado exclusivamente em `Task.WhenAll`. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/TaskWhenAll/example/example.csproj
dotnet run --project 02-AsyncAndConcurrency/TaskWhenAll/example/example.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): dispara várias tarefas e aguarda todas, contrastando com o encadeamento sequencial de `await`. O ganho de tempo é o que a saída evidencia.

## Pontos de atenção

- TFM **`net10.0`**, sem dependências externas.
- O projeto e o diretório se chamam **`example`** (minúsculo), fora do padrão PascalCase do repositório.
- **Sem README local** e **ausente do índice do README raiz** — a trilha 02 é listada com 8 projetos, sem contar este. Ao mexer aqui, considere regularizar: README local + entrada no índice + contador da categoria.
