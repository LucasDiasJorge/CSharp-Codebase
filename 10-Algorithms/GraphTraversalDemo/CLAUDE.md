# CLAUDE.md — GraphTraversalDemo

Console: busca em grafos com DFS e BFS. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/GraphTraversalDemo/GraphTraversalDemo.csproj
dotnet run --project 10-Algorithms/GraphTraversalDemo/GraphTraversalDemo.csproj
```

## Estrutura interna

- `Graph.cs` — lista de adjacências e os dois percursos. A diferença entre eles se resume à estrutura auxiliar: **pilha** (DFS, explícita ou via recursão) versus **fila** (BFS). O conjunto de visitados é o que impede laço infinito em grafo cíclico.
- `Program.cs` — monta o grafo de exemplo e imprime as ordens de visita.

BFS encontra o caminho mínimo em grafo não ponderado; DFS não. É o contraste que justifica ter os dois no mesmo arquivo.

## Pontos de atenção

- **BUILD QUEBRADO.** O `.csproj` não declara `TargetFramework` (dependia do `Directory.Build.props` removido no commit `50763d5`); `dotnet build` falha com `NETSDK1013`. Declare `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>`, ou restaure o props na raiz — lista dos 10 afetados no [CLAUDE.md](../../CLAUDE.md) raiz.
- Sem dependências externas.
