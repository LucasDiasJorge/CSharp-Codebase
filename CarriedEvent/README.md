# Event Carried State Transfer em C#

## O que é Event Carried State Transfer?

O **Event Carried State Transfer** (também conhecido como **Carried Event**) é um padrão onde os eventos carregam todos os dados necessários para que os consumidores processem a informação sem precisar consultar a origem.

## Comparação de Abordagens

### Event Notification (Tradicional)
```json
{
  "eventType": "OrderCreated",
  "orderId": "123"
}
// Consumidor precisa buscar dados do pedido
```

### Event Carried State Transfer
```json
{
  "eventType": "OrderCreated",
  "orderId": "123",
  "customerId": "456",
  "customerName": "João Silva",
  "items": [...],
  "totalAmount": 1500.00,
  "shippingAddress": {...}
}
// Consumidor tem todos os dados necessários
```

## Estrutura

```
CarriedEvent/
├── Core/
│   ├── IEvent.cs
│   ├── IEventHandler.cs
│   └── EventBus.cs
├── Examples/
│   ├── OrderCreated/
│   ├── PaymentProcessed/
│   └── InventoryUpdated/
├── Program.cs
└── README.md
```

## Benefícios

✅ **Desacoplamento** - Consumidor não precisa conhecer a origem  
✅ **Performance** - Menos chamadas de rede  
✅ **Resiliência** - Funciona mesmo se origem estiver indisponível  
✅ **Autonomia** - Serviços independentes  

## Desvantagens

⚠️ **Tamanho do evento** - Eventos maiores  
⚠️ **Consistência eventual** - Dados podem estar desatualizados  
⚠️ **Duplicação** - Mesmos dados em múltiplos eventos  

## Como Executar

```bash
cd CarriedEvent
dotnet run
```
