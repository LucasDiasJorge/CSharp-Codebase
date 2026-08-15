# CLAUDE.md — RealWorldBubbleSort

Console: consome uma API pública, ordena e faz busca binária interativa. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/RealWorldBubbleSort/RealWorldBubbleSort.csproj
dotnet run --project 10-Algorithms/RealWorldBubbleSort/RealWorldBubbleSort.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): busca usuários em `randomuser.me`, ordena por `Username` e permite busca binária sobre o resultado. A ordenação é pré-requisito da busca binária — é essa dependência que o exemplo torna concreta.

## Pontos de atenção

- **O nome mente**: não há Bubble Sort. O código usa o sort nativo do .NET (eficiente). O nome é histórico e o próprio README avisa. Não "conserte" o nome sem atualizar `.sln`, README local e índice do README raiz.
- **Exige internet** — depende da API pública `randomuser.me`. Offline, ou se a API mudar o contrato, a execução falha. `dotnet build` continua funcionando.
- **Interativo**: aguarda entrada do usuário na busca. Em validação automatizada, prefira `dotnet build`.
- TFM `net9.0`, sem pacotes externos (`HttpClient` do runtime).
