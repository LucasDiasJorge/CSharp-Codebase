# CompositionOrderFulfillment

## Visão geral

Aplicacao de console que simula o ciclo de vida de um pedido de e-commerce com itens internos.

## Conceitos abordados

- Composition entre PurchaseOrder e OrderLine.
- Regras de status do pedido (Draft, Confirmed, Cancelled).
- Ownership forte do agregado sobre seus itens.

## Objetivos de aprendizagem

- Entender como Composition controla ciclo de vida das partes.
- Aplicar regras de negocio de confirmacao e cancelamento de pedido.
- Demonstrar invalidacao de itens quando o pedido e encerrado.

## Estrutura do projeto

```text
CompositionOrderFulfillment/
+-- Models/
|   +-- Product.cs
|   \-- PurchaseOrder.cs
+-- Services/
|   \-- OrderApplicationService.cs
+-- CompositionOrderFulfillment.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/CompositionOrderFulfillment/CompositionOrderFulfillment.csproj
```

## Boas práticas e pontos de atenção

- Encapsulamento das linhas dentro da entidade principal.
- Regras de estado centralizadas no agregado.
- Snapshot de preco unitario no momento da adicao do item.

### Pontos de Atencao

- Itens nao devem existir sem o pedido em cenarios de Composition.
- Mudancas de estado devem proteger invariantes.
- Cancelamento precisa tornar linhas inativas para evitar uso posterior indevido.

## Conteúdo complementar

##### Estrutura do Projeto

```text
CompositionOrderFulfillment/
├── Models/
│   ├── Product.cs
│   └── PurchaseOrder.cs
├── Services/
│   └── OrderApplicationService.cs
├── Program.cs
├── README.md
└── CompositionOrderFulfillment.csproj
```

##### Pre-requisitos

- .NET 9.0 SDK

##### Fluxo de Negocio Demonstrado

1. Carregamento de catalogo de produtos.
2. Criacao do pedido.
3. Inclusao de itens no status Draft.
4. Confirmacao do pedido.
5. Cancelamento e invalidacao das linhas.

## Referências

- https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/
- https://www.uml-diagrams.org/composition.html
