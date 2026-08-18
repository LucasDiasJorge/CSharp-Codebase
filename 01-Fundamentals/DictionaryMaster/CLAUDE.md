# CLAUDE.md — DictionaryMaster

Console **interativo** (menu) para estudar `Dictionary<TKey, TValue>`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 01-Fundamentals/DictionaryMaster/DictionaryMaster.csproj
dotnet run --project 01-Fundamentals/DictionaryMaster/DictionaryMaster.csproj
```

## Estrutura interna

- `Program.cs` — laço de menu com cinco módulos: cadastro, busca segura (`TryGetValue`), atualização, remoção e iteração/ordenação com LINQ.
- `Models/Contato.cs` e `Models/PerguntaQuiz.cs` — modelos de apoio dos módulos de agenda e quiz.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- **Exige stdin interativo**: o programa bloqueia lendo do console. Ao validar em ambiente automatizado, prefira `dotnet build` — um `dotnet run` sem entrada fica pendurado ou encerra em erro de leitura.
