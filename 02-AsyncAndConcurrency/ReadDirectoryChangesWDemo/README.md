# ReadDirectoryChangesWDemo

Resumo prático de monitoramento de alterações em diretório no Windows usando a API nativa `ReadDirectoryChangesW` via P/Invoke em C#.

## Visão geral

Este projeto demonstra como criar um monitor de filesystem sem `FileSystemWatcher`, chamando diretamente a API Win32 `ReadDirectoryChangesW`. O foco está em entender como abrir o handle de diretório corretamente, interpretar o buffer retornado (`FILE_NOTIFY_INFORMATION`) e aplicar cancelamento seguro de operação bloqueante com `CancelIoEx`.

A execução é interativa e imprime no console eventos de criação, remoção, modificação e rename de arquivos/pastas no diretório monitorado e subpastas.

## Conceitos abordados

- Interop com Win32 em C# usando P/Invoke.
- Monitoramento de mudanças de diretório em baixo nível com `ReadDirectoryChangesW`.
- Cancelamento cooperativo de IO bloqueante com `CancellationToken` + `CancelIoEx`.
- Parsing de registros encadeados no buffer de notificação (`NextEntryOffset`).

## Objetivos de aprendizagem

- Entender quando usar API nativa em vez de abstrações de alto nível.
- Aprender a mapear contratos Win32 para tipos gerenciados com segurança.
- Aplicar padrão de monitoramento contínuo com loop de leitura e encerramento seguro.
- Identificar trade-offs de abordagem low-level para cenários de observabilidade de arquivos.

## Estrutura do projeto

```text
ReadDirectoryChangesWDemo/
|-- Program.cs
|-- ReadDirectoryChangesWDemo.csproj
`-- README.md
```

## Como executar

Executar monitorando o diretório atual:

```bash
dotnet run --project 02-AsyncAndConcurrency/ReadDirectoryChangesWDemo/ReadDirectoryChangesWDemo.csproj
```

Executar monitorando um diretório específico:

```bash
dotnet run --project 02-AsyncAndConcurrency/ReadDirectoryChangesWDemo/ReadDirectoryChangesWDemo.csproj -- "C:/temp"
```

Para encerrar, interrompa com `Ctrl+C`.

## Boas práticas e pontos de atenção

- Este exemplo é específico de Windows; em outros sistemas operacionais não há suporte para `ReadDirectoryChangesW`.
- Trate sempre códigos de erro Win32 retornados por chamadas nativas.
- Evite buffers muito pequenos em diretórios de alta taxa de alteração.
- Mantenha o parsing defensivo do buffer para evitar leitura fora dos limites.

## Conteúdo complementar

Ações notificadas no exemplo:

- `Added`
- `Removed`
- `Modified`
- `RenamedOldName`
- `RenamedNewName`

O sample configura `watchSubtree = true`, então alterações em subpastas também aparecem.

## Referências e documentação complementar

- [ReadDirectoryChangesW function (Win32)](https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-readdirectorychangesw)
- [CancelIoEx function (Win32)](https://learn.microsoft.com/windows/win32/api/ioapiset/nf-ioapiset-cancelioex)
