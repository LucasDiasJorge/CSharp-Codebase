# CLAUDE.md — PortsAndAdapters/example

API com Ports and Adapters e **seleção dinâmica de engine** por requisição. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/PortsAndAdapters/example/example.csproj
dotnet run --project 07-DesignPatterns/PortsAndAdapters/example/example.csproj
```

Requisições de exemplo em `example.http`.

## Estrutura interna

O layout de pastas é próprio e vale entender antes de editar:

- `domain/IEngine.cs` — **a porta**: contrato que o núcleo define e não depende de ninguém.
- `brazil-domain/EngineBrazil.cs`, `europe-domain/EngineEurope.cs`, `usa-domain/EngineUsa.cs` — **os adaptadores**, uma implementação por região.
- `app-core/EngineRegistry.cs` + `EngineBase.cs` — catálogo das implementações disponíveis.
- `ioc-resolver/EngineResolver.cs` — escolhe a implementação **em runtime**, a partir do dado da requisição. É a peça central: sem ela seria só DI comum.
- `Controllers/EngineController.cs` — dispara a resolução.

A inversão de dependência é o ponto: o domínio não conhece nenhuma engine concreta.

## Pontos de atenção

- TFM `net9.0`, **sem pacotes externos**.
- Pastas em kebab-case minúsculo, e o projeto se chama `example` — fora do padrão PascalCase do repositório.
- **Sem README local** — documentação em `07-DesignPatterns/PortsAndAdapters/`.
