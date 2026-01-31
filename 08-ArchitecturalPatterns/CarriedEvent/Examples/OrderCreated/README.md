# Order Created Event - Carried State Transfer

## Descrição

Demonstração do padrão Event Carried State Transfer onde o evento `OrderCreatedEvent` carrega todos os dados necessários para múltiplos consumidores.

## Dados Carregados no Evento

| Categoria | Dados |
|-----------|-------|
| **Pedido** | Id, Número, Data, Status |
| **Cliente** | Id, Nome, Email, Telefone |
| **Endereço** | Rua, Número, Cidade, Estado, CEP |
| **Itens** | Produto, SKU, Quantidade, Preço |
| **Valores** | SubTotal, Frete, Desconto, Total |

## Handlers (Consumidores)

```
                    ┌────────────────────┐
                    │ OrderCreatedEvent  │
                    │                    │
                    │ • Customer Data    │
                    │ • Address Data     │
                    │ • Items Data       │
                    │ • Payment Data     │
                    └─────────┬──────────┘
                              │
       ┌──────────────────────┼──────────────────────┐
       │                      │                      │
       ▼                      ▼                      ▼
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│ Notification│       │  Inventory  │       │  Shipping   │
│   Handler   │       │   Handler   │       │   Handler   │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Usa:        │       │ Usa:        │       │ Usa:        │
│ • Email     │       │ • Items     │       │ • Address   │
│ • Name      │       │ • SKUs      │       │ • Items     │
│ • Total     │       │ • Quantities│       │ • Customer  │
└─────────────┘       └─────────────┘       └─────────────┘
```

## Vantagens

1. **Nenhuma chamada adicional** - Handlers têm todos os dados
2. **Desacoplamento** - Não dependem do serviço de pedidos
3. **Resiliência** - Funcionam mesmo com serviço origem offline
4. **Performance** - Sem latência de rede adicional

## Estrutura

```
OrderCreated/
├── Events/
│   └── OrderCreatedEvent.cs
├── Handlers/
│   ├── NotificationHandler.cs
│   ├── InventoryHandler.cs
│   ├── ShippingHandler.cs
│   └── AnalyticsHandler.cs
└── README.md
```
