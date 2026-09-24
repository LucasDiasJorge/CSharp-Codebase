# FileChecksumDeduplicator

Console que calcula hashes por stream, localiza arquivos duplicados e mede o que costuma ficar implícito nessa tarefa: colisões, tamanho de buffer e custo de I/O.

## Visão geral

Achar arquivos duplicados parece exigir ler todos eles. Não exige. O **tamanho** vem dos metadados do sistema de arquivos, sem custo de leitura, e dois arquivos de tamanhos diferentes não podem ter o mesmo conteúdo. Filtrar por tamanho antes de hashear reduz a varredura do exemplo de **315 arquivos e 36,3 MB** para **14 arquivos e 2,2 MB** — mesmo resultado, um sexto do tempo.

O hash em si é calculado por stream, com `IncrementalHash`: o arquivo é lido em blocos e o estado do hash ocupa dezenas de bytes, independentemente do tamanho da entrada. Para um arquivo de 32 MB, a versão por stream aloca **praticamente nada** e a versão com `ReadAllBytes` aloca **32 MB** — para chegar ao mesmo hash.

Os dois últimos cenários tratam do que normalmente se aceita de ouvido. **Colisão** deixa de ser abstrata quando é medida: truncando o SHA-256 em 32 bits, duas entradas colidem em cerca de **103 mil** arquivos, não em bilhões — o paradoxo do aniversário faz a colisão chegar na *raiz* do espaço. E o **buffer** mostra o custo de I/O diretamente: 1 KB por leitura entrega 552 MB/s, 64 KB entrega 2.551 MB/s, e 1 MB praticamente empata com 64 KB.

## Conceitos abordados

- Hash incremental por stream com `IncrementalHash` e custo de memória fixo.
- Metadados de arquivo como filtro barato antes da leitura do conteúdo.
- Tamanho igual não implica conteúdo igual; hash igual é evidência, não prova.
- Comparação byte a byte como desempate definitivo.
- Paradoxo do aniversário e espaço de hash.
- Diferença entre colisão por acaso e colisão construída (MD5, SHA-1).
- Efeito do tamanho do buffer sobre chamadas de sistema e vazão.
- Efeito avalanche: um byte muda o hash inteiro.

## Objetivos de aprendizagem

- Calcular hash de arquivos de qualquer tamanho sem carregá-los em memória.
- Projetar uma varredura de duplicatas que leia o mínimo necessário do disco.
- Justificar a escolha do algoritmo de hash pelo uso, não por hábito.
- Escolher tamanho de buffer com base em medição, não em palpite.
- Saber quando "mesmo hash" ainda exige confirmação byte a byte.

## Estrutura do projeto

```text
FileChecksumDeduplicator/
|-- Demo/
|   |-- DeduplicatorDemoRunner.cs
|   `-- SampleTreeFactory.cs
|-- Hashing/
|   |-- DuplicateFinder.cs
|   `-- FileHasher.cs
|-- FileChecksumDeduplicator.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run -c Release --project 11-Utilities/FileChecksumDeduplicator/FileChecksumDeduplicator.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 11-Utilities/FileChecksumDeduplicator/FileChecksumDeduplicator.csproj
```

Não exige serviço externo. O programa **cria 315 arquivos (cerca de 36 MB, incluindo um de 32 MB)** em uma pasta `dedup-demo` dentro do diretório de saída da compilação e **apaga tudo ao final**. Use `-c Release`, porque quase todos os cenários medem tempo.

## Boas práticas e pontos de atenção

- Filtre pelo que é barato antes do que é caro: tamanho (metadados) → hash de um pedaço → hash completo → comparação byte a byte. Cada etapa só recebe o que a anterior não descartou.
- Use `IncrementalHash` ou `HashAlgorithm.ComputeHash(Stream)` em arquivo. `ReadAllBytes` num arquivo grande é um `OutOfMemoryException` esperando o dia certo — e acima de 2 GB nem array existe.
- `FileOptions.SequentialScan` informa ao sistema operacional o padrão de acesso, permitindo leitura antecipada. É barato de colocar e mede-se em arquivos grandes.
- Escolha o algoritmo pelo uso. Para deduplicação local sem adversário, hashes rápidos e não criptográficos (xxHash, BLAKE3) são mais adequados. Para integridade contra manipulação, SHA-256. **MD5 e SHA-1 não servem onde alguém pode escolher o conteúdo** — colisões para eles são construídas, publicadas e baratas.
- Hash igual é evidência muito forte, não prova. Em deduplicação que **apaga** arquivos, confirme byte a byte antes de excluir; o custo aparece só nos poucos candidatos que chegaram lá.
- Meça o buffer em vez de adivinhar. Abaixo de alguns KB o custo por chamada de sistema domina; acima de algumas dezenas de KB o ganho some e só sobra memória ocupada.
- Trate erros por arquivo, não por varredura: arquivo bloqueado, permissão negada e link simbólico são normais numa árvore real, e nenhum deles deve abortar o resto.
- Cuidado com links: contar hard links ou symlinks como duplicatas e "limpar" um deles pode apagar o único conteúdo real.
- Em disco rápido, paralelizar o hash ajuda; em HDD, competir por cabeça de leitura piora. A medição precisa ser feita no hardware de destino.

## Conteúdo complementar

**1. Hash por stream contra carregar o arquivo inteiro** — arquivo de 32 MB:

| Estratégia | Alocado | Tempo |
|---|---:|---:|
| Streaming, buffer de 64 KB | **~0 MB** | 19,9ms |
| `ReadAllBytes` + `HashData` | **32,0 MB** | 17,6ms |

O hash resultante é idêntico. A diferença é que o custo de memória da versão por stream **não depende do arquivo**: ela processaria 50 GB com o mesmo buffer de 64 KB.

**2. Achar duplicatas** — 315 arquivos, 36,3 MB no total:

| Estratégia | Arquivos lidos | Bytes lidos | Tempo | Grupos |
|---|---:|---:|---:|---:|
| Hashear tudo | 315 | 36,3 MB | 28,0ms | 4 |
| Agrupar por tamanho primeiro | **14** | **2,2 MB** | **5,3ms** | 4 |

Os mesmos 4 grupos, lendo 6% dos bytes. O arquivo de 32 MB tem tamanho único: a segunda estratégia **nunca o abre**.

**3. O que cada coisa garante**:

```text
mesmo-tamanho-a e mesmo-tamanho-b: 65.536 bytes os dois (iguais: True)
  hash a: 1B047B8F635F7013... | hash b: A959A4BBEE7312B8... | iguais: False
```

Os arquivos diferem em **um único byte, no fim** — e o hash muda por inteiro, desde o primeiro dígito. É o efeito avalanche, e é o que torna o prefixo do hash utilizável como atalho de comparação.

**4. Colisão, medida** — truncando o SHA-256:

| Bits | Colidiu em | Esperado (~1,25 × √2ⁿ) |
|---:|---:|---:|
| 16 | 385 entradas | 321 |
| 24 | 4.895 entradas | 5.134 |
| 32 | 103.802 entradas | 82.136 |

A colisão chega na **raiz** do espaço, não na metade dele: 32 bits não aguentam nem uma pasta de fotos. Pelo mesmo cálculo, 256 bits dariam ~4,26 × 10³⁸ arquivos — é por isso que o SHA-256 é tratado como identidade de conteúdo.

Os valores medidos oscilam em torno do esperado (aqui, 385 contra 321 e 103.802 contra 82.136) porque é **uma** amostra de um processo aleatório, não uma média.

**5. Tamanho do buffer** — mesmo arquivo de 32 MB:

| Buffer | Chamadas de `Read` | Tempo | Vazão |
|---:|---:|---:|---:|
| 1 KB | 32.768 | 57,9ms | 552 MB/s |
| 4 KB | 8.192 | 23,2ms | 1.378 MB/s |
| 64 KB | 512 | 12,5ms | 2.551 MB/s |
| 1 MB | 32 | 12,2ms | 2.633 MB/s |

De 1 KB para 64 KB a vazão quase quintuplica; de 64 KB para 1 MB o ganho é ruído. O arquivo já está no cache do sistema operacional nesta medição — em disco frio os números caem, mas a **forma** da curva é a mesma.

Relação com os vizinhos: `CsvStreamingProcessor` aplica a mesma ideia de streaming a linhas de texto em vez de bytes. `CompressDecompress` também trabalha sobre streams de arquivo, com outro objetivo.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/api/system.security.cryptography.incrementalhash
- https://learn.microsoft.com/dotnet/api/system.io.fileoptions
- https://en.wikipedia.org/wiki/Birthday_attack
- https://www.rfc-editor.org/rfc/rfc6151 (por que MD5 não serve mais para integridade)
