# PortsAndAdapters

Exemplo didatico de Ports and Adapters (Hexagonal Architecture) com selecao de engine por contrato de dominio e resolucao via IoC.

## Visao geral

Este projeto demonstra como desacoplar regras de aplicacao da infraestrutura usando portas (`IEngine`) e adaptadores concretos (`EngineBrazil`, `EngineEurope`, `EngineUsa`).

A API recebe o nome da engine e um comando de operacao, resolve a implementacao registrada no container e executa o fluxo, retornando qual engine foi usada na operacao.

## Conceitos abordados

- Ports and Adapters com interface de dominio (`IEngine`).
- Inversao de dependencia e resolucao de adaptadores via DI.
- Controller ASP.NET Core para validar dinamicamente qual implementacao foi usada.

## Objetivos de aprendizagem

- Entender como trocar implementacoes sem alterar a camada de entrada.
- Praticar composicao por contrato para cenarios multi-engine.
- Testar roteamento de comportamento por chave de selecao em tempo de execucao.

## Estrutura do projeto

```text
PortsAndAdapters/
|-- example/
|   |-- app-core/
|   |   |-- EngineBase.cs
|   |   `-- EngineFactory.cs
|   |-- brazil-domain/
|   |   `-- EngineBrazil.cs
|   |-- europe-domain/
|   |   `-- EngineEurope.cs
|   |-- usa-domain/
|   |   `-- EngineUsa.cs
|   |-- domain/
|   |   `-- IEngine.cs
|   |-- ioc-resolver/
|   |   `-- EngineResolver.cs
|   |-- Controllers/
|   |   `-- EngineController.cs
|   |-- Program.cs
|   `-- example.csproj
`-- README.md
```

## Como executar

Projeto executavel:

```bash
dotnet run --project 07-DesignPatterns/PortsAndAdapters/example/example.csproj
```

Build isolado:

```bash
dotnet build 07-DesignPatterns/PortsAndAdapters/example/example.csproj
```

Teste rapido dos endpoints:

```bash
GET  /api/engine/available
POST /api/engine/execute
```

Body de exemplo:

```json
{
  "engineName": "EngineEurope",
  "command": "simulate-payment"
}
```

## Boas praticas e pontos de atencao

- Mantenha os nomes das engines unicos para evitar sobrescrita no dicionario da factory.
- Para novos adaptadores, implemente `IEngine` e registre no `EngineResolver`.
- A selecao da engine e case-insensitive para facilitar testes e integracoes.

## Conteudo complementar

Resposta esperada do endpoint de execucao:

```json
{
  "engineName": "EngineEurope",
  "command": "simulate-payment",
  "statusCode": 0,
  "executedAtUtc": "2026-06-07T12:34:56.789Z"
}
```

## Referencias e documentacao complementar

- https://learn.microsoft.com/aspnet/core
- https://learn.microsoft.com/dotnet/core/extensions/dependency-injection
- https://alistair.cockburn.us/hexagonal-architecture/
