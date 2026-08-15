# CLAUDE.md — YieldReturnDemo

Console sobre `yield return`: máquina de estados gerada pelo compilador, execução preguiçosa e armadilhas reais. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/YieldReturnDemo/YieldReturnDemo.csproj
dotnet run --project 01-Fundamentals/YieldReturnDemo/YieldReturnDemo.csproj
```

## Estrutura interna

É o projeto mais estruturado da trilha. `Program.cs` apenas despacha para os módulos de `Demos/`, cada um isolando um aspecto:

- `BasicsDemo` — o iterador mais simples.
- `DeferredExecutionDemo` — nada executa até a primeira iteração.
- `ManualEnumeratorDemo` — `MoveNext()` chamado à mão, expondo a máquina de estados.
- `InfiniteSequenceDemo` — sequência infinita consumida com `Take`.
- `StreamingPipelineDemo` — encadeamento preguiçoso sem materializar coleção intermediária.
- `PitfallsDemo` — as armadilhas (re-enumeração repetindo efeitos colaterais, exceção que só dispara na iteração, e não na chamada).

`LazyOperators.cs` reúne os operadores preguiçosos próprios usados pelos demos.

Ao adicionar conteúdo, crie um novo arquivo em `Demos/` e registre-o no despacho do `Program.cs`; não infle os demos existentes.

## Pontos de atenção

- TFM **`net10.0`** (a maioria da trilha está em `net9.0`). Preserve ao editar.
- Sem dependências externas.
