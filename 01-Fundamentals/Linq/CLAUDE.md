# CLAUDE.md — Linq

Console de consultas LINQ sobre coleções em memória. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/Linq/Linq.csproj
dotnet run --project 01-Fundamentals/Linq/Linq.csproj
```

## Estrutura interna

- `Models/{Cliente,Funcionario,Pedido,Produto}.cs` — massa de dados fixa que serve de fonte para todas as consultas.
- `Program.cs` — bateria de consultas cobrindo projeção, filtro, agrupamento, junção e agregação, em sintaxe de método e de query.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas — tudo roda em `IEnumerable`, não há provedor `IQueryable` nem banco.
- Este é o projeto onde a exceção à regra do `var` legitimamente aparece: projeções para **tipo anônimo** (`select new { ... }`) não têm tipo explícito escrevível. Em qualquer outro ponto, use tipo explícito.
- Para o lado `IQueryable`/expression trees, ver `PredicateAggregationDemo`.
