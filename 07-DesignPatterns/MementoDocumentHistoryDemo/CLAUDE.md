# CLAUDE.md — MementoDocumentHistoryDemo

Console com o padrão Memento: salvar e restaurar estado sem expô-lo, com o custo de memória visível. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/MementoDocumentHistoryDemo/MementoDocumentHistoryDemo.csproj
dotnet run --project 07-DesignPatterns/MementoDocumentHistoryDemo/MementoDocumentHistoryDemo.csproj
```

Roda cinco cenários e termina. Sem serviço externo.

## Estrutura interna

O mecanismo de opacidade é o assunto do projeto e **não deve ser afrouxado**:

- `Documents/IDocumentMemento` expõe só `Label`, `CapturedAt` e `ApproximateSizeInChars`. Acrescentar `Content` ou `Tags` aqui destrói o padrão — vira um DTO público com indireção.
- `DocumentEditor.DocumentMemento` é **classe aninhada privada**. Ninguém fora do editor consegue nomear o tipo, e é por isso que o cast em `Restore` só compila lá dentro. Mover essa classe para fora do editor quebra a garantia.

`DocumentEditor.Save` faz `new List<string>(_tags)`; `SaveShallow` compartilha a referência. **Os dois existem de propósito** — o cenário 3 compara os dois lado a lado. Não "corrigir" o `SaveShallow`.

`History/DocumentHistory` é o caretaker e só manuseia a interface. `TotalRetainedChars` existe para tornar o custo um número.

## Pontos de atenção

- TFM `net10.0`. Pacote: `Microsoft.Extensions.Logging.Console` 10.0.12.
- Os números do README vêm da execução: cenário 4 imprime 600/1300/2100/3000 chars retidos para um documento de 900. Alterar os tamanhos (`new string('x', 500)`, `'y', 100`) invalida a tabela.
- O limite padrão de `DocumentHistory` é 5, mas o cenário 4 usa 10 e o cenário 5 usa 3, passados no construtor. Três valores diferentes, de propósito; conferir qual antes de mexer.
- A cópia profunda aqui é de um nível: copia a `List<string>`, e `string` é imutável. **Uma lista de objetos mutáveis exigiria copiar também os objetos** — está dito no README como cuidado, mas o código não demonstra esse caso.
- `Restore` valida o tipo e lança `ArgumentException` para memento de outro originator. Não trocar por cast direto, que daria `InvalidCastException` sem explicação.
- `Program.cs` tem `await Task.Delay(200)` antes da linha final, pela fila do provider de console.
- **Fronteira com os vizinhos**: [CommandUndoRedoDemo](../CommandUndoRedoDemo/CLAUDE.md) resolve o mesmo problema — voltar atrás — guardando operações em vez de estados. Os dois READMEs se referenciam e trazem uma tabela comparativa; ao mexer em um, conferir se a comparação no outro ainda vale. Não fundir os dois projetos: o contraste é o valor didático.
