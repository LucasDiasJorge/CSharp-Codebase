# Dictionary Merge — Sincronização de Status com ConcurrentDictionary

## O que faz

Sincroniza status de notas fiscais entre um estado local e um sistema remoto usando `ConcurrentDictionary`. Identifica diferenças, aplica atualizações e mantém ambos os lados consistentes.

## Por que essa abordagem é boa

| Aspecto | Detalhe |
|---------|---------|
| **Lookup** | `TryGetValue` é O(1) — acesso direto por hash |
| **Sync** | O(R + T) — uma passada no remoto + uma no local |
| **Thread-safety** | `ConcurrentDictionary` elimina locks manuais |
| **Clareza** | `Sync` detecta diferenças, `ApplyUpdates` aplica — responsabilidades separadas |
| **Deduplicação** | `HashSet<string>` no `SyncResult` evita duplicatas |

### Comparativo de complexidade

| Abordagem | Complexidade |
|-----------|-------------|
| Loop aninhado + `FirstOrDefault` (List) | O(R * T) — quadrática |
| **Índice com ConcurrentDictionary** | **O(R + T) — linear** |

Com 100K registros, a diferença é de **minutos vs milissegundos**.

## Estrutura

```
NotaFiscalSyncManager   -> gerencia estado local e sincroniza com remoto
SyncResult              -> resultado do diff (NeedsUpdate, NewInRemote, NotInRemote, AlreadySync)
Program                 -> exemplo de uso
```

## Como rodar

```bash
cd 11-Utilities/DictionaryMerge
dotnet run
```

## Exemplo

```csharp
NotaFiscalSyncManager manager = new NotaFiscalSyncManager();

manager.SetStatus("NF001", "Processando");
manager.SetStatus("NF002", "Aprovada");
manager.SetStatus("NF003", "Rejeitada");

ConcurrentDictionary<string, string> remote = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
remote["NF002"] = "Aprovada";
remote["NF004"] = "Aprovada";

SyncResult result = manager.Sync(remote);
manager.ApplyUpdates(remote, result);

// result.NeedsUpdate   -> NFs com status diferente
// result.NewInRemote   -> NFs que só existem no remoto
// result.NotInRemote   -> NFs que só existem no local
// result.AlreadySync   -> NFs já iguais
```

## Conceitos aplicados

- `ConcurrentDictionary` para thread-safety sem lock
- `HashSet` para conjuntos sem duplicata
- `StringComparer.OrdinalIgnoreCase` para comparação eficiente de chaves
- Separação entre detecção de diferenças e aplicação de mudanças
