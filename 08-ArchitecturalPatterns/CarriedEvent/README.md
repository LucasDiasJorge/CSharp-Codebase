# Event Carried State Transfer em C#

## Visão geral

O **Event Carried State Transfer** (também conhecido como **Carried Event**) é um padrão onde os eventos carregam todos os dados necessários para que os consumidores processem a informação sem precisar consultar a origem.

## Conceitos abordados

- Exemplo didático sobre Event Carried State Transfer em C# no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Event Carried State Transfer em C# se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
CarriedEvent/
+-- Core/
|   +-- CarriedStateEvent.cs
|   +-- IEvent.cs
|   +-- IEventBus.cs
|   +-- IEventHandler.cs
|   \-- InMemoryEventBus.cs
+-- Examples/
|   \-- OrderCreated/
+-- CarriedEvent.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/CarriedEvent/CarriedEvent.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Event Notification (Tradicional)

```json
{
  "eventType": "OrderCreated",
  "orderId": "123"
}
// Consumidor precisa buscar dados do pedido
```

##### Event Carried State Transfer

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

##### Estrutura

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

##### Benefícios

✅ **Desacoplamento** - Consumidor não precisa conhecer a origem  
✅ **Performance** - Menos chamadas de rede  
✅ **Resiliência** - Funciona mesmo se origem estiver indisponível  
✅ **Autonomia** - Serviços independentes

##### Desvantagens

⚠️ **Tamanho do evento** - Eventos maiores  
⚠️ **Consistência eventual** - Dados podem estar desatualizados  
⚠️ **Duplicação** - Mesmos dados em múltiplos eventos
