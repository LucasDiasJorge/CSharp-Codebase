# Order Created Event - Carried State Transfer

## Visão geral

Projeto didático do CSharp-101 dedicado a Order Created Event - Carried State Transfer, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Order Created Event - Carried State Transfer no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Order Created Event - Carried State Transfer se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
OrderCreated/
+-- Events/
|   \-- OrderCreatedEvent.cs
\-- Handlers/
    +-- AnalyticsHandler.cs
    +-- InventoryHandler.cs
    +-- NotificationHandler.cs
    \-- ShippingHandler.cs
```

## Como executar

Consulte o código desta pasta e os projetos relacionados antes de executar comandos específicos.

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

Demonstração do padrão Event Carried State Transfer onde o evento `OrderCreatedEvent` carrega todos os dados necessários para múltiplos consumidores.

##### Dados Carregados no Evento

| Categoria | Dados |
|-----------|-------|
| **Pedido** | Id, Número, Data, Status |
| **Cliente** | Id, Nome, Email, Telefone |
| **Endereço** | Rua, Número, Cidade, Estado, CEP |
| **Itens** | Produto, SKU, Quantidade, Preço |
| **Valores** | SubTotal, Frete, Desconto, Total |

##### Handlers (Consumidores)

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

##### Vantagens

1. **Nenhuma chamada adicional** - Handlers têm todos os dados
2. **Desacoplamento** - Não dependem do serviço de pedidos
3. **Resiliência** - Funcionam mesmo com serviço origem offline
4. **Performance** - Sem latência de rede adicional

##### Estrutura

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
