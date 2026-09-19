# CommandUndoRedoDemo

Console que encapsula operações de edição como objetos de comando e implementa histórico, undo e redo sobre eles.

## Visão geral

O padrão Command transforma uma operação em objeto. Em vez de chamar `documento.Inserir(0, "Ola")` direto, cria-se um `InsertTextCommand` que sabe executar essa chamada — e, principalmente, sabe desfazê-la. Uma vez que a operação é um objeto, ela pode ser guardada em lista, enfileirada, repetida e revertida.

O trabalho de verdade não está no método `Execute`, e sim no `Undo`. Inserir é fácil de reverter: basta remover o que foi inserido, e posição e tamanho já são conhecidos. Remover é outra história — para desfazer é preciso ter guardado o texto apagado no instante da execução. Se o comando não captura esse estado, a informação some e o undo se torna impossível.

Essa captura é também o custo do padrão. `ReplaceAllCommand` precisa guardar uma cópia do documento inteiro; um histórico de cem operações dessas guarda cem documentos. Por isso todo editor tem um limite de histórico: ele não é uma limitação arbitrária, é a troca entre poder desfazer e consumir memória.

O exemplo mostra ainda duas coisas que costumam surpreender. Um comando composto desfaz seus passos na **ordem inversa** da execução, porque cada um pressupõe o estado deixado pelo anterior. E executar qualquer coisa nova depois de um undo **descarta o redo** — o histórico é uma linha, não uma árvore.

## Conceitos abordados

- Command como reificação de uma operação: a chamada vira objeto.
- Separação entre quem pede (invoker), quem sabe fazer (receiver) e o pedido em si.
- Captura de estado no comando como condição para o undo.
- Custo de memória proporcional ao que a operação destrói.
- Comando composto e desfazer na ordem inversa.
- Descarte do redo ao executar um comando novo.
- Limite de histórico como troca entre reversibilidade e memória.
- Duas pilhas como implementação suficiente de undo/redo.

## Objetivos de aprendizagem

- Decidir o que cada comando precisa guardar para ser reversível.
- Reconhecer quais operações são caras de desfazer e por quê.
- Implementar undo/redo sem espalhar estado pelo receptor.
- Entender por que o redo desaparece quando o histórico ganha um ramo novo.

## Estrutura do projeto

```text
CommandUndoRedoDemo/
|-- Commands/
|   |-- CompositeCommand.cs
|   |-- DeleteRangeCommand.cs
|   |-- InsertTextCommand.cs
|   |-- IUndoableCommand.cs
|   `-- ReplaceAllCommand.cs
|-- Demo/
|   `-- EditorDemoRunner.cs
|-- Editor/
|   |-- CommandHistory.cs
|   `-- TextDocument.cs
|-- CommandUndoRedoDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/CommandUndoRedoDemo/CommandUndoRedoDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 07-DesignPatterns/CommandUndoRedoDemo/CommandUndoRedoDemo.csproj
```

Não exige serviço externo. A execução é imediata e roda os cinco cenários em sequência.

## Boas práticas e pontos de atenção

- Decida o que capturar no `Execute`, não no construtor. O estado a reverter só existe no momento da execução: `DeleteRangeCommand` só sabe o que foi apagado depois de apagar.
- Nem toda operação é reversível. Enviar e-mail, cobrar cartão, chamar API de terceiro — para essas, o `Undo` honesto é uma operação compensatória (estorno, cancelamento), não um retorno ao estado anterior. Um comando que não pode reverter deveria dizer isso, não fingir.
- Desfaça composto na ordem inversa. Cada passo pressupõe o estado deixado pelo anterior; reverter na mesma ordem corrompe o resultado.
- Limpe o redo ao executar algo novo. Manter o redo depois de um comando novo permitiria "refazer" uma operação que pressupunha um estado que deixou de existir.
- Limite o histórico. Sem limite, cada comando retém para sempre o estado que precisa para reverter — é vazamento de memória com outro nome.
- Mantenha o receptor ignorante do padrão. `TextDocument` não conhece comando nem histórico; expõe só operações primitivas. É isso que permite trocar o mecanismo de undo sem tocar no domínio.
- O invoker é o único lugar que conhece as duas pilhas. Comandos não sabem onde estão no histórico, e é melhor assim.
- Command combina bem com fila e log: como a operação é um objeto, dá para serializá-la, persistir e reexecutar. É a ponte para event sourcing.

## Conteúdo complementar

Papéis do padrão neste exemplo:

| Papel | Classe | Responsabilidade |
|---|---|---|
| Command | `IUndoableCommand` | Contrato de executar e desfazer |
| Concrete Command | `InsertTextCommand`, `DeleteRangeCommand`, `ReplaceAllCommand` | Sabem fazer e reverter uma operação |
| Composite | `CompositeCommand` | Trata vários comandos como um |
| Receiver | `TextDocument` | Sabe fazer o trabalho; ignora o padrão |
| Invoker | `CommandHistory` | Executa e mantém as pilhas de undo/redo |

Custo de reversão por tipo de operação:

| Comando | O que precisa guardar | Custo |
|---|---|---|
| `InsertTextCommand` | Nada além dos parâmetros | Constante |
| `DeleteRangeCommand` | O trecho removido | Proporcional ao removido |
| `ReplaceAllCommand` | O documento inteiro anterior | Proporcional ao documento |

Saída dos cenários (trechos):

```text
=== 1. Executar, desfazer e refazer ===
Documento: "Ola, mundo" | undo=2 redo=0
Desfeito: inserir ", mundo" na posicao 3
Documento: "Ola"        | undo=1 redo=1
Refeito:  inserir ", mundo" na posicao 3
Documento: "Ola, mundo" | undo=2 redo=0

=== 3. Comando composto: varios passos, um unico undo ===
Executado: formatar como titulo
Documento: "# OLA, MUNDO #" | undo=3 redo=0
Desfeito:  formatar como titulo
Documento: "Ola, mundo"     | undo=2 redo=1

=== 4. Executar algo novo apos um undo descarta o redo ===
Apos o undo:         1 operacao disponivel para refazer
Apos o novo comando: 0 operacoes para refazer
Tentativa de redo:   nao ha o que refazer

=== 5. Limite do historico ===
Depois de 12 comandos, o historico guarda 10 — os mais antigos nao podem mais ser desfeitos
```

O cenário 3 substitui o documento inteiro e depois o restaura: os três passos internos foram revertidos de trás para frente, e o texto original voltou exatamente como estava.

Quando o padrão compensa:

| Situação | Vale a pena? |
|---|---|
| Undo/redo de verdade | Sim, é o caso canônico |
| Fila de operações a executar depois | Sim, o comando serializa bem |
| Log de auditoria do que foi pedido | Sim, o objeto já descreve a intenção |
| Uma única chamada de método, sem histórico | Não, é indireção sem ganho |

Relação com os vizinhos da trilha: `DesignPattern` reúne implementações introdutórias de vários padrões GoF; este projeto isola Command e vai além do básico, tratando de composto, descarte de redo e limite de histórico. `MementoDocumentHistoryDemo` resolve o mesmo problema — voltar atrás — por outro caminho: salvando estados em vez de operações.

## Referências e documentação complementar

- https://refactoring.guru/design-patterns/command
- https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/
- https://en.wikipedia.org/wiki/Command_pattern
