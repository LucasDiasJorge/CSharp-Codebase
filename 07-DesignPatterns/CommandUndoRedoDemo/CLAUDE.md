# CLAUDE.md — CommandUndoRedoDemo

Console com o padrão Command aplicado a um editor de texto: histórico, undo, redo, comando composto e limite de histórico. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/CommandUndoRedoDemo/CommandUndoRedoDemo.csproj
dotnet run --project 07-DesignPatterns/CommandUndoRedoDemo/CommandUndoRedoDemo.csproj
```

Roda cinco cenários em sequência e termina sozinho. Sem serviço externo.

## Estrutura interna

Os três papéis estão separados de propósito e **não devem se misturar**: `Editor/TextDocument` é o receptor e não conhece comando nem histórico; `Commands/*` sabem executar e reverter; `Editor/CommandHistory` é o invoker e é o único que conhece as duas pilhas.

`Commands/DeleteRangeCommand` captura o texto removido **dentro de `Execute`**, não no construtor — o estado a reverter só existe no momento da execução. É o ponto que distingue o padrão de uma indireção inútil.

`Commands/ReplaceAllCommand` guarda o documento inteiro anterior. Existe para tornar visível o custo de memória do padrão; não "otimizar" guardando um diff, isso esconderia a lição.

`CompositeCommand.Undo` percorre a lista **de trás para frente**. Inverter isso corrompe o resultado e o exemplo passa despercebido, porque o cenário 3 ainda "parece" funcionar em casos simples.

`CommandHistory.Execute` limpa a pilha de redo quando há algo nela. É o comportamento correto (histórico é linha, não árvore) e está demonstrado no cenário 4.

## Pontos de atenção

- TFM `net10.0`, alinhado a `StrategyResolver` e aos demais projetos recentes da trilha. Pacote: `Microsoft.Extensions.Logging.Console` 10.0.12.
- `CommandHistory` tem `limit: 10`, passado em `Program.cs`. O cenário 5 executa 12 comandos e o README afirma que sobram 10 — alterar o limite exige atualizar o texto.
- `TrimToLimit` reconstrói a pilha para descartar o item mais antigo. `Stack<T>` não remove do fundo; a reconstrução é O(n) e aceitável no tamanho deste exemplo, mas não é o que se faria com histórico grande (ali caberia uma lista circular ou `LinkedList`).
- `Program.cs` tem `await Task.Delay(200)` antes da linha final. É por causa da fila do provider de console do logging — sem isso, "Fim dos cenarios." sai no meio das mensagens. Mesmo motivo do delay em `AsyncLockingDemo`.
- Os cenários compartilham o **mesmo documento e o mesmo histórico**, em sequência: o estado de um entra no seguinte. Os textos citados no README dependem dessa ordem; não reordenar os cenários sem refazer a saída documentada.
- **Fronteira com os vizinhos**: `DesignPattern` tem implementações introdutórias de vários GoF. Aqui o foco é só Command, com o que o básico não cobre. [MementoDocumentHistoryDemo](../MementoDocumentHistoryDemo/CLAUDE.md) ataca o mesmo problema — voltar atrás — guardando estados em vez de operações; manter a distinção clara entre os dois.
