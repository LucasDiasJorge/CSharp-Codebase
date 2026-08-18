# CLAUDE.md — Decorator

Console: comportamento adicionado a um notificador por composição em camadas. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Structural/Decorator/Decorator.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Structural/Decorator/Decorator.csproj
```

## Estrutura interna

- `INotifier.cs` — contrato comum.
- `BaseNotifier.cs` — implementação base.
- `NotifierDecorator.cs` — decorador abstrato que **implementa a interface e embrulha outra instância dela**. É essa dupla natureza que permite empilhar.
- `Decorators/{EmailDecorator,SmsDecorator,SlackDecorator,PriorityDecorator}.cs` — camadas combináveis em qualquer ordem, em tempo de execução.

Compare com herança: com 4 canais, cobrir todas as combinações exigiria uma explosão de subclasses. O exemplo existe para tornar esse contraste concreto.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Cada decorador deve delegar ao embrulhado antes ou depois do seu trabalho; esquecer a delegação corta a cadeia silenciosamente.
