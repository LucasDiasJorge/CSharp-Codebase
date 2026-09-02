# GenericConstraintsDemo

Projeto console que demonstra classes e metodos genericos, variance e constraints para criar APIs reutilizaveis com seguranca de tipos.

## Visao geral

O exemplo modela um pequeno catalogo em memoria. Ele usa um repositorio generico para armazenar entidades, uma fabrica com constraint `new()`, um relatorio com constraints de interface e utilitarios que separam casos para tipos de referencia e tipos valor.

Tambem ha uma demonstracao de covariance com `out T` para leitura e contravariance com `in T` para escrita em um sink de auditoria.

## Conceitos abordados

- Classes genericas com `where T : class`.
- Metodos genericos com constraints `class`, `new()` e interfaces.
- Constraint `struct` combinada com `IFormattable`.
- Covariance com `out T` em APIs somente leitura.
- Contravariance com `in T` em APIs consumidoras.

## Objetivos de aprendizagem

- Entender como constraints tornam APIs genericas mais seguras e expressivas.
- Separar genericos para leitura, escrita, criacao e calculo.
- Reconhecer quando usar `class`, `struct`, `new()` e constraints de interface.
- Aplicar variance para reaproveitar contratos sem casts manuais.

## Estrutura do projeto

```text
GenericConstraintsDemo/
|-- Contracts/
|   |-- IEntity.cs
|   |-- IEntitySink.cs
|   |-- IPricedItem.cs
|   `-- IReadableCatalog.cs
|-- Demo/
|   `-- GenericDemoRunner.cs
|-- Infrastructure/
|   |-- EntityAuditSink.cs
|   `-- InMemoryRepository.cs
|-- Models/
|   |-- Product.cs
|   `-- StockSnapshot.cs
|-- Services/
|   `-- CatalogReport.cs
|-- Utilities/
|   |-- EntityFactory.cs
|   |-- MetricFormatter.cs
|   |-- PriceCalculator.cs
|   `-- ReferenceInspector.cs
|-- GenericConstraintsDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/GenericConstraintsDemo/GenericConstraintsDemo.csproj
```

Para validar compilacao:

```bash
dotnet build 01-Fundamentals/GenericConstraintsDemo/GenericConstraintsDemo.csproj
```

## Boas praticas e pontos de atencao

- Use constraints para declarar capacidades necessarias em vez de aceitar qualquer `T`.
- Prefira `out T` quando a interface apenas produz valores e `in T` quando ela apenas consome valores.
- Evite `new()` quando a criacao exigir invariantes complexas; nesse caso, prefira uma factory explicita.
- Constraints de interface deixam metodos genericos mais testaveis e menos dependentes de tipos concretos.

## Conteudo complementar

Mapeamento dos exemplos:

```text
InMemoryRepository<TEntity>  -> class + interface constraint
EntityFactory.CreateEntity   -> class + IEntity + new()
MetricFormatter.FormatStruct -> struct + IFormattable
IReadableCatalog<out T>      -> covariance para leitura
IEntitySink<in T>            -> contravariance para escrita
```

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters
- https://learn.microsoft.com/dotnet/csharp/programming-guide/concepts/covariance-contravariance/
- https://learn.microsoft.com/dotnet/csharp/programming-guide/generics/
