# CLAUDE.md — Mediator

Console: sala de chat onde os participantes se comunicam através de um mediador. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Behavioral/Mediator/MediatoR.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/Mediator/MediatoR.csproj
```

**Atenção**: a pasta é `Mediator`, o `.csproj` é `MediatoR.csproj`. Use como está.

## Estrutura interna

- `IMediator.cs` / `ChatRoomMediator.cs` — o hub que conhece todos os participantes.
- `IUser.cs` / `User.cs` — participantes que conhecem **apenas** o mediador, nunca uns aos outros.

O padrão converte um grafo N:N de dependências em uma estrela. É essa mudança topológica o conteúdo do exemplo.

## Pontos de atenção

- TFM `net9.0`, **sem o pacote MediatR** — a implementação é manual, apesar do nome do `.csproj`. Não confunda com a biblioteca homônima nem a adicione aqui.
- O mediador tende a virar objeto-deus conforme cresce; é o custo conhecido do padrão e vale mencionar se o exemplo for estendido.
