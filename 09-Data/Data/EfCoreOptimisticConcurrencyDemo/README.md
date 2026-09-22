# EfCoreOptimisticConcurrencyDemo

Console que reproduz uma atualização perdida, detecta o conflito com um concurrency token e aplica as três estratégias de resolução, mais retry.

## Visão geral

Dois usuários leem o mesmo registro, cada um decide uma alteração, os dois gravam. Sem nenhuma proteção, o segundo simplesmente escreve por cima — e ninguém fica sabendo. O estoque que deveria cair de 10 para 5 cai para 7, porque uma das baixas desapareceu.

Vale notar quando isso acontece. O EF Core gera `UPDATE` apenas das colunas que mudaram, então dois usuários alterando **campos diferentes** não se atropelam. O problema aparece quando os dois mexem no **mesmo** campo a partir de um valor lido antes — o clássico read-modify-write.

A proteção é um concurrency token: uma coluna que entra na cláusula `WHERE` de todo `UPDATE`. Se outro processo já gravou, o valor mudou, nenhuma linha é afetada e o EF levanta `DbUpdateConcurrencyException`. Não há travamento — é otimista justamente porque assume que o conflito é raro e só o detecta quando acontece.

Detectar é metade do trabalho. O que fazer depois tem três respostas possíveis, e o exemplo aplica as três: descartar minhas alterações e ficar com o banco, sobrescrever o banco com as minhas, ou mesclar campo a campo. A quarta opção — reler e reaplicar a intenção — é o retry, e é a mais adequada quando a operação é uma variação relativa ("tirar 3 do estoque") e não um valor absoluto.

## Conceitos abordados

- Atualização perdida em read-modify-write concorrente.
- `UPDATE` apenas das colunas alteradas, e o que isso implica.
- Concurrency token com `IsConcurrencyToken` e o `WHERE` que ele gera.
- `DbUpdateConcurrencyException` e `exception.Entries`.
- Resolução por store wins, client wins e merge campo a campo.
- Retry reaplicando a intenção, não o valor absoluto.
- Ausência de `rowversion` no SQLite e como suprir.

## Objetivos de aprendizagem

- Reconhecer quando duas gravações concorrentes se perdem em silêncio.
- Escolher a estratégia de resolução conforme o significado da operação.
- Entender por que repetir o valor absoluto em um retry reintroduz o problema.
- Saber o que o provider faz por você e o que fica por sua conta.

## Estrutura do projeto

```text
EfCoreOptimisticConcurrencyDemo/
|-- Data/
|   `-- ShopDbContext.cs
|-- Demo/
|   `-- ConcurrencyDemoRunner.cs
|-- Model/
|   `-- Entities.cs
|-- EfCoreOptimisticConcurrencyDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 09-Data/Data/EfCoreOptimisticConcurrencyDemo/EfCoreOptimisticConcurrencyDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 09-Data/Data/EfCoreOptimisticConcurrencyDemo/EfCoreOptimisticConcurrencyDemo.csproj
```

Não exige serviço externo — SQLite em arquivo local, recriado a cada execução e não versionado.

## Boas práticas e pontos de atenção

- Concorrência otimista não trava nada. Ela não impede o conflito: apenas garante que você saiba que ele aconteceu, em vez de perder dados em silêncio.
- Escolha a estratégia pelo significado da operação, não por conveniência. "O último que salvou vence" é uma decisão de negócio, e precisa ser tomada de propósito.
- Em retry, reaplique a **intenção**. Se a operação era "tirar 3 do estoque", releia e subtraia 3 do valor atual. Repetir "estoque = 7" traz de volta exatamente a atualização perdida que o token detectou.
- Limite as tentativas. Um retry sem teto vira laço infinito sob contenção alta.
- O EF Core só atualiza colunas alteradas. Isso reduz colisões, mas não protege o caso que importa — dois processos alterando o mesmo campo.
- No SQLite não há `rowversion`. O token precisa ser mantido pela aplicação, e isso tem de valer para **toda** gravação: um caminho que esqueça de incrementar desliga a detecção sem erro.
- No SQL Server, prefira `rowversion` mantido pelo banco. Token gerenciado pela aplicação depende de todo código passar pelo mesmo lugar — e alguém sempre escreve um `UPDATE` direto.
- Merge campo a campo exige comparar proposto com original. Só mantenha o valor local nas propriedades que **você** alterou; nas demais, aceite o banco.
- Concorrência otimista serve para conflito raro. Se dois processos disputam a mesma linha o tempo todo, o custo do retry supera o do lock — e a resposta pode ser pessimista, ou repensar o modelo.

## Conteúdo complementar

Cenários e resultados:

**1. Sem token — atualização perdida**

```text
A e B leem estoque 10
A separa 2 e grava 8
B separa 3 e grava 7
estoque final: 7    (o correto seria 5)
```

Nenhuma exceção, nenhum log de erro. A baixa de A desapareceu.

**2. Com token — conflito detectado**

```text
A e B leem a versao 0
A grava -> versao 1
B tenta gravar -> DbUpdateConcurrencyException
```

O `UPDATE` de B procurava `WHERE Id = 1 AND Version = 0`; nenhuma linha correspondia.

**3 a 5. As três resoluções**

| Estratégia | O que faz | Resultado no exemplo |
|---|---|---|
| Banco vence | Recarrega do banco, abandona o local | B descarta 275 e fica com 199 |
| Cliente vence | Aceita a versão atual como base e regrava | B sobrescreve com 275 |
| Mesclar | Mantém só o que eu alterei, aceita o resto | preço 199 (de A) **e** estoque 77 (de B) |

O merge é o único em que nenhuma alteração se perde — mas só funciona quando os dois mexeram em campos diferentes.

**6. Retry**

```text
outro processo grava preco 189 no meio do caminho
tentativa 1: falha por conflito
tentativa 2: grava. estoque 7, preco 189
```

O retry releu e subtraiu 3 do valor **atual**. O preço de quem interferiu foi preservado, porque B nunca o alterou.

Quando usar cada abordagem:

| Situação | Abordagem |
|---|---|
| Campos diferentes, ambos válidos | Merge |
| A alteração mais recente é a autoritativa | Cliente vence |
| O banco tem a verdade e minha cópia envelheceu | Banco vence |
| A operação é relativa (somar, subtrair) | Retry reaplicando a intenção |
| Conflito frequente na mesma linha | Reveja o modelo, ou use lock |

Token por provider:

| Provider | Mecanismo |
|---|---|
| SQL Server | `rowversion`, mantido pelo banco |
| PostgreSQL | `xmin` como token, via `UseXminAsConcurrencyToken()` |
| SQLite | Sem equivalente nativo; a aplicação mantém o token |

Relação com os vizinhos da trilha: `EfCoreRelationshipsDemo` cobre modelagem e carregamento. `MySqlNamedAdvisoryLockReservation` mostra a alternativa pessimista — travar de fato, em vez de detectar depois. `EventSourcingBankAccountDemo` (trilha 08) aplica a mesma ideia de versão esperada a um fluxo de eventos.

## Referências e documentação complementar

- https://learn.microsoft.com/ef/core/saving/concurrency
- https://learn.microsoft.com/ef/core/modeling/concurrency
- https://learn.microsoft.com/ef/core/saving/disconnected-entities
