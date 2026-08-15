# CLAUDE.md — SlidingWindows

Console: técnica de janela deslizante sobre arrays. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/SlidingWindows/SlidingWindows.csproj
dotnet run --project 10-Algorithms/SlidingWindows/SlidingWindows.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): soma máxima de subarray de tamanho fixo. A ideia central é não recalcular a janela inteira a cada passo — soma-se o elemento que entra e subtrai-se o que sai, levando o custo de O(n·k) para O(n).

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Cobre a janela de **tamanho fixo**. A variante de tamanho variável (expandir/contrair por condição) não está implementada e seria a extensão natural.
