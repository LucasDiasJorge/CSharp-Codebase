# EventSourcingBankAccountDemo

Console que modela uma conta bancária por eventos: o estado é derivado do fluxo, não armazenado, com concorrência otimista por versão e snapshots como atalho de leitura.

## Visão geral

Em um modelo tradicional, a conta tem uma coluna `saldo` e cada operação a sobrescreve. O valor anterior desaparece, e o histórico — se existir — é uma tabela paralela que pode divergir do saldo real.

No event sourcing, o que se grava são os fatos: conta aberta, depósito de 500, saque de 200. O saldo não é armazenado em lugar nenhum — ele é calculado aplicando os eventos em ordem. O histórico não é um extra mantido à parte: ele **é** o dado, e por isso não tem como divergir.

Isso resolve alguns problemas e cria outros. A auditoria sai de graça e dá para reconstruir o estado em qualquer ponto do passado, bastando parar de aplicar eventos mais cedo. Em troca, toda leitura precisa reprocessar o fluxo, e uma conta com milhares de movimentos fica cara de carregar. A resposta é o snapshot — um estado materializado em uma versão, que permite ler só os eventos posteriores. No exemplo, isso leva a leitura de 201 eventos para 1.

A concorrência funciona por versão esperada. Dois processos carregam a conta na versão 3; o primeiro grava e o fluxo vai para 4; o segundo tenta gravar esperando a 3 e é rejeitado. Não há travamento — quem chega atrasado relê e decide de novo, e a decisão pode mudar: no exemplo, o saque que era válido com saldo 1300 deixa de ser com 1000.

## Conceitos abordados

- Evento como fato imutável, nomeado no passado.
- Estado derivado do fluxo (fold), não persistido.
- Reidratação do agregado a partir dos eventos.
- Separação entre validar comando e aplicar evento.
- Concorrência otimista por versão esperada.
- Snapshot como atalho de leitura, não fonte da verdade.
- Auditoria e viagem no tempo como consequências, não recursos extras.
- Armazenamento append-only.

## Objetivos de aprendizagem

- Modelar um agregado cujo estado nasce dos eventos.
- Entender por que `Apply` não valida e `Comando` valida.
- Implementar concorrência otimista sem travar nada.
- Avaliar quando o snapshot é necessário e por que ele não substitui o fluxo.
- Reconhecer o que o padrão custa antes de adotá-lo.

## Estrutura do projeto

```text
EventSourcingBankAccountDemo/
|-- Demo/
|   `-- EventSourcingDemoRunner.cs
|-- Domain/
|   `-- BankAccount.cs
|-- Events/
|   `-- AccountEvents.cs
|-- Store/
|   `-- EventStore.cs
|-- EventSourcingBankAccountDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/EventSourcingBankAccountDemo/EventSourcingBankAccountDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 08-ArchitecturalPatterns/EventSourcingBankAccountDemo/EventSourcingBankAccountDemo.csproj
```

Não exige serviço externo. Roda os cinco cenários e termina.

## Boas práticas e pontos de atenção

- Nomeie eventos no passado e trate-os como imutáveis. `DepositoRealizado`, não `RealizarDeposito`. Não se edita nem se apaga um evento: corrigir é acrescentar outro que compense.
- Valide no comando, nunca no `Apply`. Eventos antigos precisam continuar aplicáveis mesmo que a regra de negócio tenha mudado desde então; validar na aplicação quebra a releitura de fluxos históricos.
- Guarde a versão esperada ao gravar. Sem isso, duas escritas concorrentes se sobrepõem e o saldo fica errado sem erro nenhum.
- Depois de um conflito, releia e **redecida**. Reexecutar cegamente o mesmo comando é tão errado quanto ignorar o conflito: a operação pode ter deixado de ser válida, e é exatamente o que o cenário 3 mostra.
- Snapshot é atalho, não verdade. Se o snapshot divergir do fluxo, o fluxo está certo. Apagar todos os snapshots tem de ser uma operação segura.
- Versione o formato dos eventos desde o começo. Eles vão viver anos; um campo novo precisa ter valor padrão sensato para os eventos antigos que não o têm.
- Event sourcing não é para tudo. Ele compensa onde o histórico tem valor de negócio — conta bancária, pedido, apólice. Para cadastro de produto, é complexidade sem retorno.
- O fluxo cresce para sempre. Retenção, arquivamento e snapshots deixam de ser opcionais em qualquer sistema de vida longa.
- Leitura é o ponto fraco. Consultar "todas as contas com saldo acima de X" exige projeções materializadas — que é onde CQRS entra.

## Conteúdo complementar

Eventos do exemplo:

| Evento | Efeito no estado |
|---|---|
| `AccountOpened` | Define titular e saldo inicial |
| `MoneyDeposited` | Soma ao saldo |
| `MoneyWithdrawn` | Subtrai do saldo |
| `AccountFrozen` | Bloqueia movimentação |
| `AccountUnfrozen` | Libera movimentação |

Resultados observados:

**1 e 2. Estado derivado e auditoria**

```text
em memoria:            saldo 1300.00, versao 3
recarregado do fluxo:  saldo 1300.00, versao 3

v1: conta aberta para Ana com 1000.00
v2: deposito de 500.00
v3: saque de 200.00
```

Nenhuma coluna de saldo foi gravada, e nenhuma tabela de log foi escrita.

**3. Concorrência otimista**

```text
A e B carregam na versao 3, saldo 1300.00
A saca 300 e grava        -> fluxo na versao 4
B tenta gravar            -> Conflito: esperava a versao 3, o fluxo esta na 4
B rele                    -> saldo agora e 1000.00
B tenta o saque de 1200   -> Saldo insuficiente: saldo 1000.00, pedido 1200.00
```

O saque de 1200 era perfeitamente válido quando B leu. Depois do conflito, deixou de ser — e é por isso que reexecutar cegamente seria um erro.

**4. Snapshot**

```text
sem snapshot:        201 eventos lidos
com snapshot:          1 evento lido
releitura completa:  202 eventos lidos

saldo com snapshot 2155.00 = saldo por releitura completa 2155.00
```

O snapshot reduziu a leitura em duas ordens de grandeza e chegou exatamente ao mesmo estado.

**5. Viagem no tempo**

```text
na versao 1: saldo 1000.00
na versao 3: saldo 1300.00
na versao 4: saldo 1000.00
```

O que o padrão dá e o que cobra:

| | Modelo tradicional | Event sourcing |
|---|---|---|
| Estado atual | Uma leitura direta | Reprocessar o fluxo (ou snapshot) |
| Histórico | Tabela à parte, pode divergir | É o próprio dado |
| Estado no passado | Perdido | Reconstruível |
| Consulta por atributo | Trivial | Exige projeção materializada |
| Concorrência | Lock ou coluna de versão | Versão esperada no append |
| Crescimento | Limitado pelo número de registros | Cresce para sempre |

Relação com os vizinhos da trilha: `CQRSDemo` separa leitura de escrita, que é o complemento natural — projeções resolvem a fraqueza de consulta deste modelo. `CarriedEvent` trata de eventos persistidos em outro contexto. `SagaPattern` coordena transações distribuídas, que costumam ser dirigidas por eventos como estes.

## Referências e documentação complementar

- https://martinfowler.com/eaaDev/EventSourcing.html
- https://microservices.io/patterns/data/event-sourcing.html
- https://learn.microsoft.com/azure/architecture/patterns/event-sourcing
