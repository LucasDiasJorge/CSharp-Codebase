# MementoDocumentHistoryDemo

Console que salva e restaura o estado de um documento sem expor sua estrutura interna, e mostra o que o padrão Memento custa em memória.

## Visão geral

O Memento resolve um conflito entre dois objetivos: poder voltar a um estado anterior e manter o estado encapsulado. A solução ingênua — expor os campos para que alguém os copie — resolve o primeiro destruindo o segundo.

O padrão separa três papéis. O **originator** é o objeto cujo estado se quer salvar; só ele sabe o que compõe esse estado e só ele consegue lê-lo de volta. O **memento** é o snapshot, opaco por construção. O **caretaker** guarda os snapshots sem nunca conseguir olhar dentro deles.

Em C#, a opacidade se consegue com uma classe aninhada privada implementando uma interface pública estreita. O histórico recebe um `IDocumentMemento`, que expõe apenas rótulo, horário e tamanho; o tipo concreto é privado do editor, e ninguém de fora consegue sequer nomeá-lo — quanto mais ler seus campos.

Há duas armadilhas que o exemplo demonstra em vez de descrever. A primeira é a cópia rasa: um memento que guarda a referência de uma coleção mutável não salvou nada, porque o documento continua alterando exatamente aquele objeto. A segunda é o custo — cada snapshot é uma cópia completa, e um documento de 900 caracteres com quatro pontos de restauração retém 3000 caracteres no histórico.

## Conceitos abordados

- Originator, memento e caretaker, e o que cada um pode fazer.
- Classe aninhada privada + interface pública estreita como mecanismo de opacidade.
- Cópia profunda como condição para o snapshot ser um snapshot.
- Custo de memória proporcional ao tamanho do estado vezes o número de pontos salvos.
- Limite de histórico e descarte do mais antigo.
- Restauração de estado composto (conteúdo, coleção e posição).
- Diferença entre salvar estado (Memento) e salvar operações (Command).

## Objetivos de aprendizagem

- Implementar undo por snapshot sem abrir a estrutura interna do objeto.
- Reconhecer quando uma "cópia" é apenas uma referência compartilhada.
- Estimar o custo de memória de um histórico de snapshots.
- Escolher entre Memento e Command conforme o que é mais barato guardar.

## Estrutura do projeto

```text
MementoDocumentHistoryDemo/
|-- Demo/
|   `-- MementoDemoRunner.cs
|-- Documents/
|   |-- DocumentEditor.cs
|   `-- IDocumentMemento.cs
|-- History/
|   `-- DocumentHistory.cs
|-- MementoDocumentHistoryDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/MementoDocumentHistoryDemo/MementoDocumentHistoryDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 07-DesignPatterns/MementoDocumentHistoryDemo/MementoDocumentHistoryDemo.csproj
```

Não exige serviço externo. Roda os cinco cenários e termina.

## Boas práticas e pontos de atenção

- Mantenha o memento opaco. Se o caretaker consegue ler o estado, o padrão virou um DTO público com passos a mais — e o encapsulamento que justificava tudo se perdeu.
- Copie de verdade. Guardar a referência de uma lista, dicionário ou objeto mutável não salva estado nenhum: o originator continua alterando aquele mesmo objeto, e a "restauração" devolve o estado atual.
- Copie o que for mutável, em profundidade suficiente. `string` e tipos de valor não precisam; coleções e objetos mutáveis, sim. Uma lista de objetos mutáveis exige copiar também os objetos, não só a lista.
- Limite o histórico. Snapshot completo por ponto de restauração significa que a memória cresce com o tamanho do documento vezes o número de saves.
- Escolha entre Memento e Command pelo que é mais barato. Guardar a operação (Command) é melhor quando a mudança é pequena e o estado é grande; guardar o estado (Memento) é melhor quando a operação é difícil de inverter ou quando restaurar precisa ser instantâneo.
- Snapshots incrementais são a otimização natural: guardar a diferença em vez do estado inteiro, com snapshots completos esparsos para não precisar reconstruir desde o começo. É a mesma ideia de snapshot em event sourcing.
- Valide o tipo ao restaurar. `Restore` recebe a interface e faz cast para o tipo aninhado; um memento de outro originator precisa ser rejeitado com erro claro, não com `InvalidCastException`.
- Cuidado com estado que não é seu. Um memento que captura um `HttpClient`, uma conexão ou um handle de arquivo não está salvando estado — está prolongando a vida de um recurso.

## Conteúdo complementar

Papéis e o que cada um alcança:

| Papel | Classe | Consegue ler o estado? |
|---|---|---|
| Originator | `DocumentEditor` | Sim — é o único |
| Memento | `DocumentEditor.DocumentMemento` (aninhada, privada) | É o estado |
| Interface pública | `IDocumentMemento` | Só rótulo, horário e tamanho |
| Caretaker | `DocumentHistory` | Não |

Resultados observados:

**1. Restauração de estado composto**

```text
apos v1:            "Relatorio trimestral"             tags=[rascunho]           cursor=20
apos edicoes:       "Relatorio trimestral - revisado"  tags=[rascunho, revisao]  cursor=0
apos restaurar v1:  "Relatorio trimestral"             tags=[rascunho]           cursor=20
```

Os três componentes do estado voltaram juntos, inclusive o cursor.

**2. O que o caretaker enxerga**

```text
historico ve: confidencial (29 chars, 20:50:37)
```

Rótulo, tamanho e horário. O conteúdo não aparece porque a interface não o expõe.

**3. Cópia rasa versus profunda**

```text
copia rasa,     apos restaurar: tags=[original, adicionada-depois]
copia profunda, apos restaurar: tags=[original]
```

Na versão rasa, a tag adicionada **depois** do snapshot sobreviveu à restauração — prova de que o snapshot nunca foi um snapshot.

**4. Custo de memória**

```text
snap-1: 600 chars retidos
snap-2: 1300
snap-3: 2100
snap-4: 3000

documento com 900 chars, historico retendo 3000
```

O histórico já ocupa mais que o triplo do documento.

**5. Limite do histórico**

```text
Limite de 3 atingido: snapshot "v1" descartado (9 chars liberados).
```

Memento comparado com Command:

| | Memento | Command |
|---|---|---|
| O que guarda | Estado completo | A operação e o necessário para revertê-la |
| Custo por ponto | Tamanho do estado | Tamanho da mudança |
| Restaurar | Instantâneo, aplica o snapshot | Reexecuta os `Undo` em ordem inversa |
| Operação difícil de inverter | Indiferente | Problema sério |
| Estado grande, mudança pequena | Caro | Barato |
| Precisa saber *o que* mudou | Não registra | Registra naturalmente |

Os dois padrões resolvem "voltar atrás" por caminhos opostos, e a escolha é quase sempre econômica.

Relação com os vizinhos da trilha: `CommandUndoRedoDemo` resolve o mesmo problema guardando operações em vez de estados — vale ler os dois em sequência. `DesignPattern/Behavioral` reúne implementações introdutórias de outros padrões comportamentais.

## Referências e documentação complementar

- https://refactoring.guru/design-patterns/memento
- https://en.wikipedia.org/wiki/Memento_pattern
- https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/nested-types
