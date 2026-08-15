# CLAUDE.md — CQRSDemo

Console: separação entre Commands (escrita) e Queries (leitura). Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/CQRSDemo/CQRSDemo.csproj
dotnet run --project 08-ArchitecturalPatterns/CQRSDemo/CQRSDemo.csproj
```

## Estrutura interna

A simetria das pastas é o conteúdo:

- `Commands/` — `CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`. Mudam estado, não retornam dados.
- `Queries/` — `GetProductByIdQuery`, `GetAllProductsQuery`, `GetLowStockProductsQuery` + `ProductDto`. Retornam dados, não mudam estado.
- `Handlers/` — **um handler por command/query**, sem classe compartilhada entre os dois lados.
- `Infrastructure/InMemoryDatabase.cs` — store único.

Note que as queries devolvem `ProductDto`, não a entidade: o modelo de leitura é próprio, o que é a razão de existir do CQRS.

Ao adicionar operação, crie o par command/query **e** seu handler; não reaproveite handler entre os lados.

## Pontos de atenção

- TFM `net8.0`. Referencia **`LucasSDK.Logging` 1.0.0**, pacote próprio do autor e não da Microsoft. O build resolve localmente, mas se o restore falhar em outra máquina ou em ambiente limpo, é a causa provável.
- Store em memória: um único banco para leitura e escrita. CQRS "completo" separaria também os stores — aqui a separação é só de modelo.
