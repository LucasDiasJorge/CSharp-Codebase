# CLAUDE.md — CircuitBreakerDemo

Console: disjuntor implementado à mão contra um serviço instável. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/CircuitBreakerDemo/CircuitBreakerDemo.csproj
dotnet run --project 08-ArchitecturalPatterns/CircuitBreakerDemo/CircuitBreakerDemo.csproj
```

## Estrutura interna

- `ServicoInstavel.cs` — falha de forma controlada, para provocar o disjuntor.
- `CircuitBreaker.cs` — a máquina de três estados: **Closed** (passa), **Open** (rejeita imediatamente, sem tentar) e **Half-Open** (deixa passar uma sonda para testar recuperação).

O estado Half-Open é o que separa um disjuntor de um simples contador de erros: é ele que permite voltar sozinho ao normal. Ao editar, preserve a transição Open → Half-Open por tempo.

## Pontos de atenção

- TFM `net8.0`, **sem Polly** — a implementação é manual e didática. Em produção usaria-se `Microsoft.Extensions.Resilience`/Polly; não troque aqui, o valor está em ver a máquina de estados.
- A saída depende de temporização e aleatoriedade: execuções diferentes mostram sequências diferentes.
