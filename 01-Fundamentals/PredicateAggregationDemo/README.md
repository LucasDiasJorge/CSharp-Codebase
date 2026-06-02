# Predicate Aggregation Demo

## Visão geral

Projeto console didático que demonstra como agregar predicados em C# para montar filtros a partir de condições opcionais. O exemplo começa com delegates `Func<T, bool>` aplicados a coleções em memória e avança para árvores de expressão `Expression<Func<T, bool>>`, usadas por provedores `IQueryable`.

O projeto inclui um `PredicateBuilder` nativo e sem pacotes externos. Ele recompõe os parâmetros internos das árvores de expressão ao combinar condições com `AND` e `OR`, mantendo a composição adequada para cenários como consultas com Entity Framework Core.

## Conceitos abordados

- Combinação estática de delegates `Func<T, bool>` com operadores lógicos.
- Construção incremental de filtros opcionais com `Expression<Func<T, bool>>`.
- Agregação dinâmica com `AND` a partir de um filtro de produtos.
- Agregação dinâmica com `OR` a partir de uma lista de categorias.
- Substituição de parâmetros em árvores de expressão com `ExpressionVisitor`.
- Diferença entre filtros executados em memória e expressões consumidas por `IQueryable`.

## Objetivos de aprendizagem

- Combinar regras booleanas simples para filtrar coleções em memória.
- Montar predicados dinâmicos sem duplicar combinações de condições.
- Entender por que árvores de expressão exigem recomposição dos parâmetros.
- Reconhecer quando uma solução nativa é suficiente e quando uma biblioteca como LINQKit pode ser útil.

## Estrutura do projeto

```text
PredicateAggregationDemo/
|-- Expressions/
|   `-- PredicateBuilder.cs
|-- Models/
|   |-- Product.cs
|   `-- ProductFilter.cs
|-- PredicateAggregationDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/PredicateAggregationDemo/PredicateAggregationDemo.csproj
```

## Boas práticas e pontos de atenção

- Inicie agregações com `True<T>()` quando as próximas condições forem adicionadas com `AND`.
- Inicie agregações com `False<T>()` quando as próximas condições forem adicionadas com `OR`.
- Prefira expressões que o provedor de consulta consiga traduzir quando o destino for um banco de dados.
- Evite compilar a expressão antes de passá-la para um `IQueryable`; a compilação transforma a árvore em delegate e impede a tradução da consulta.
- O exemplo usa `AsQueryable()` sobre dados em memória para permanecer autocontido. Em EF Core, aplique a expressão diretamente ao `DbSet`.

## Conteúdo complementar

### Uso com Entity Framework Core

O builder nativo deste projeto pode ser aplicado diretamente a uma consulta:

```csharp
Expression<Func<Product, bool>> predicate = CreateProductPredicate(filter);
List<Product> products = context.Products
    .Where(predicate)
    .ToList();
```

### Alternativa com LINQKit

LINQKit oferece um `PredicateBuilder` pronto e o método `AsExpandable()`, útil quando a composição utiliza expressões que precisam ser expandidas antes da tradução pelo provedor. Após adicionar o pacote adequado da família LINQKit para o provedor utilizado:

```csharp
using LinqKit;

ExpressionStarter<Product> predicate = PredicateBuilder.New<Product>(true);
predicate = predicate.And(product => product.IsActive);

List<Product> products = context.Products
    .AsExpandable()
    .Where(predicate)
    .ToList();
```

Este projeto mantém a implementação nativa para tornar visível o trabalho de recomposição feito em uma árvore de expressão.

## Referências e documentação complementar

- [Árvores de expressão em C#](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- [Consultas no Entity Framework Core](https://learn.microsoft.com/ef/core/querying/)
- [LINQKit](https://github.com/scottksmith95/LINQKit)
