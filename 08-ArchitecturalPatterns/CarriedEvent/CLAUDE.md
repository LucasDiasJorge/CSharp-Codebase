# CLAUDE.md — CarriedEvent

Console: **Event Carried State Transfer** — eventos que carregam todo o estado necessário. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/CarriedEvent/CarriedEvent.csproj
dotnet run --project 08-ArchitecturalPatterns/CarriedEvent/CarriedEvent.csproj
```

## Estrutura interna

- `Core/` — `IEvent`, `IEventHandler`, `IEventBus` e `InMemoryEventBus`. Infraestrutura mínima de publicação/assinatura.
- `Core/CarriedStateEvent.cs` — a peça-chave: o evento leva os **dados**, não apenas o id.
- `Examples/OrderCreated/` — `OrderCreatedEvent` consumido por quatro handlers independentes (`Inventory`, `Shipping`, `Notification`, `Analytics`).

O ponto: nenhum handler faz callback ao serviço de origem para buscar detalhes. Isso elimina o acoplamento temporal (o consumidor funciona mesmo se o produtor estiver fora do ar), ao custo de eventos maiores e de dados possivelmente defasados.

Ao adicionar um handler, ele deve se bastar com o payload do evento. Se precisar consultar a origem, o padrão foi violado.

## Pontos de atenção

- TFM `net8.0`, sem dependências externas — o event bus é em memória, não há broker. Para broker real, ver a trilha `05-Messaging`.
