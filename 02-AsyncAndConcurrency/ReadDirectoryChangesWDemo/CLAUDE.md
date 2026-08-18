# CLAUDE.md — ReadDirectoryChangesWDemo

Console que monitora um diretório chamando a API Win32 `ReadDirectoryChangesW` via P/Invoke, sem `FileSystemWatcher`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/ReadDirectoryChangesWDemo/ReadDirectoryChangesWDemo.csproj
dotnet run --project 02-AsyncAndConcurrency/ReadDirectoryChangesWDemo/ReadDirectoryChangesWDemo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`) com três partes que precisam ser lidas juntas:

1. **Abertura do handle** — `CreateFile` com `FILE_FLAG_BACKUP_SEMANTICS`, obrigatório para obter handle de diretório.
2. **Leitura do buffer** — `ReadDirectoryChangesW` bloqueia e devolve uma cadeia de `FILE_NOTIFY_INFORMATION` de tamanho variável; o parsing percorre a lista por offsets, não por registros de tamanho fixo.
3. **Cancelamento** — `CancelIoEx` para desbloquear a chamada pendente de forma segura no encerramento.

## Pontos de atenção

- **Somente Windows.** É P/Invoke direto na Win32; não roda em Linux/macOS apesar do TFM ser `net9.0` (sem sufixo `-windows`).
- Erros de marshalling aqui corrompem memória silenciosamente em vez de lançar exceção. Ao mexer nas structs ou nas assinaturas `DllImport`, confira layout e `CharSet` contra a documentação da API.
- Sem dependências externas.
