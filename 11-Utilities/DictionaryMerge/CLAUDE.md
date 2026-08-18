# CLAUDE.md — DictionaryMerge

Console: sincronização de status de notas fiscais com `ConcurrentDictionary`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/DictionaryMerge/DictionaryMerge.csproj
dotnet run --project 11-Utilities/DictionaryMerge/DictionaryMerge.csproj
```

## Estrutura interna

- `NotaFiscalSyncManager.cs` — o mesclador. O ponto técnico é `AddOrUpdate`: decide numa **única operação atômica** se insere ou atualiza. A alternativa ingênua (`ContainsKey` seguido de escrita) tem janela de corrida entre as duas chamadas.
- `SyncResult.cs` — resultado da mesclagem (o que entrou, o que foi atualizado, o que ficou).
- `Program.cs` — cenário de sincronização.

## Pontos de atenção

- TFM **`net10.0`** (boa parte da trilha está em `net9.0`).
- Sem dependências externas.
- O delegate de update de `AddOrUpdate` pode ser **invocado mais de uma vez** sob contenção; mantenha-o puro, sem efeito colateral.
