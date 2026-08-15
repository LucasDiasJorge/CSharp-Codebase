# CLAUDE.md — Two-Sum

Console: problema clássico Two Sum. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/Two-Sum/Two-Sum.csproj
dotnet run --project 10-Algorithms/Two-Sum/Two-Sum.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): dado um array e um alvo, encontrar os dois índices cuja soma bate. A solução com dicionário troca a busca aninhada O(n²) por uma passagem O(n), guardando o complemento visto.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- **Sem README local** e **ausente do índice do README raiz** — a trilha 10 está listada com 5 projetos, sem contar este. O nome com hífen (`Two-Sum`) também foge do PascalCase do repositório. Ao mexer aqui, considere regularizar: README local + entrada no índice + contador da categoria.
