# CLAUDE.md — PredicateAggregationDemo

Console que compõe filtros opcionais em um único predicado, de `Func<T,bool>` até `Expression<Func<T,bool>>`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/PredicateAggregationDemo/PredicateAggregationDemo.csproj
dotnet run --project 01-Fundamentals/PredicateAggregationDemo/PredicateAggregationDemo.csproj
```

## Estrutura interna

O projeto tem duas metades, e a segunda é a que importa:

1. **Delegates** — `Func<T,bool>` combinados em memória. Simples, mas não traduzível para SQL.
2. **Expression trees** — `Expressions/PredicateBuilder.cs` implementa `And`/`Or` **sem pacote externo**, reescrevendo os parâmetros internos das duas árvores para um parâmetro comum. Sem essa reescrita, `Expression.AndAlso` sobre lambdas de origens distintas gera uma árvore com dois parâmetros e o provedor `IQueryable` falha em runtime.

- `Models/ProductFilter.cs` — objeto com os critérios opcionais (nulos = não filtrar).
- `Models/Product.cs` — massa de dados.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas — o `PredicateBuilder` é próprio, não é o do LinqKit. Não troque por pacote sem necessidade didática: o ponto do exemplo é ver a reescrita de parâmetro acontecendo.
- O cenário-alvo é EF Core (`IQueryable`), mas **não há banco aqui**; a demonstração roda sobre coleção em memória.
