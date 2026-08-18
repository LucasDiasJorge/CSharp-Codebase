# CLAUDE.md — CompositionOrderFulfillment

Console que demonstra **composição**: os itens do pedido nascem e morrem com o pedido. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/CompositionOrderFulfillment/CompositionOrderFulfillment.csproj
dotnet run --project 01-Fundamentals/CompositionOrderFulfillment/CompositionOrderFulfillment.csproj
```

## Estrutura interna

- `Models/PurchaseOrder.cs` — **dono** das partes: cria os itens internamente e não expõe a coleção para mutação externa.
- `Models/Product.cs` — parte componente, sem identidade fora do pedido.
- `Services/OrderApplicationService.cs` — camada de aplicação que dirige o ciclo de vida do pedido.

O contraste com `AggregationDepartmentManagement` é o núcleo didático: lá a parte preexiste ao todo; aqui ela é criada pelo todo.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Não exponha a coleção interna de itens como mutável — isso quebraria a demonstração de composição.
