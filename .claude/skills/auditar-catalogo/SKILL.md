---
name: auditar-catalogo
description: Audita a consistência do catálogo inteiro do CSharp-Codebase — projetos ausentes do índice do README raiz, contadores de categoria errados, projetos fora da .sln, .csproj sem TargetFramework, README/CLAUDE.md faltando e uso de var por projeto. Use quando o pedido for auditar, revisar o catálogo, checar o índice, conferir contadores, achar projetos não documentados ou levantar a saúde geral do repositório.
---

# Auditar catálogo

Varredura de consistência dos 125 projetos. Para revisar **um** projeto, use `/revisar-sample`.

## Rodar

```bash
powershell -ExecutionPolicy Bypass -File .\.claude\skills\auditar-catalogo\scripts\Audit-Catalog.ps1
```

Opções: `-SkipVarScan` pula a seção G (a mais lenta, ~1 min); `-RepoRoot <caminho>` audita outro
checkout. Saída 0 = consistente nas seções A/D/E; saída 1 = há achados acionáveis.

O script **não altera nada**. Ele relata; a correção é decisão sua e do usuário.

## O que cada seção significa

| Seção | Verificação | Interpretação |
|---|---|---|
| **A** | `.csproj` em disco sem entrada no índice do README raiz | Achado real — o catálogo é o produto, projeto invisível é projeto incompleto |
| **B** | Entrada do índice sem pasta/projeto em disco | Achado real — nome errado, projeto removido ou renomeado |
| **C** | Contador declarado vs. entradas listadas vs. `.csproj` | Só `declarado ≠ entradas` é erro certo; ver abaixo |
| **D** | `.csproj` fora da `CSharp-Codebase.sln` | Não quebra build, mas some do `dotnet sln list` |
| **E** | `.csproj` sem `<TargetFramework>` | Build quebrado — `NETSDK1013` |
| **F** | Pasta sem `README.md` / `CLAUDE.md` | Ver ressalva de pasta-mãe abaixo |
| **G** | Ocorrências de `var` por projeto | Heurística ruidosa — nunca corrija em massa sem ler |

## Ler o resultado sem inventar problema

Três armadilhas de interpretação. Errar nelas gera diff grande e errado.

### C — `entradas ≠ csproj` costuma ser legítimo

O índice mistura três coisas: projetos com `.csproj`, pastas agrupadoras (`Kafka`, `Caching`,
`MySimpleSdk` aparecem como item **e** listam subprojetos aninhados) e itens de estudo sem
`.csproj` (`BlockchainDemo`). A contagem em disco jamais bate exatamente.

**Só `declarado ≠ entradas` é erro garantido** — é o número no título da categoria contra os itens
que a própria seção lista. Esse conserta-se editando o título.

### F — README de pasta-mãe cobre subprojetos

24 pastas não têm `README.md` próprio, e a maioria é subprojeto coberto pelo README acima
(`05-Messaging/Kafka/Send` está documentado em `05-Messaging/Kafka/README.md`). O script anota
`[coberto por .../README.md]` nesses casos — não são achados. Os sem anotação são os reais.

### G — `var` é heurística, não veredito

O regex pega a palavra em comentários, strings e nomes como `variavel`, e não distingue tipo
anônimo de LINQ (a única exceção permitida). Os contadores servem para priorizar, não para
concluir. Corrigir exige abrir o arquivo. Ver `/revisar-sample`, seção 1.

## Baseline conhecido (2026-08-19)

Rodada de referência com o repositório limpo:

```text
csproj em disco ................ 125
ausentes do indice (A) ......... 6
entradas orfas do indice (B) ... 3
ausentes da solucao (D) ........ 10
sem TargetFramework (E) ........ 10
sem README local (F) ........... 24
sem CLAUDE.md local (F) ........ 0
```

**A (6):** `02-AsyncAndConcurrency/TaskWhenAll/example`, `07-DesignPatterns/DesignPattern/Creational/Registry`,
`07-DesignPatterns/PortsAndAdapters/example`, `10-Algorithms/Two-Sum`,
`11-Utilities/TaskScheduler/{production,project}`.

**B (3):** `BlockchainDemo` (item de estudo sem `.csproj` — esperado); `Sockets.Client` e
`Sockets.Server`, que o índice cita mas em disco são `01-Fundamentals/Sockets/{Client,Server}`
e **não contêm `.csproj` nenhum** — o sample de sockets está no catálogo sem projeto real.

**C:** 10 das 14 categorias com `declarado ≠ entradas`.

Se um número subir acima do baseline depois de uma alteração sua, a alteração introduziu o
achado. Se estiver no baseline, é dívida preexistente — reporte, não conserte de passagem.

## Corrigir

Só quando o usuário pedir, e um eixo por vez. Ordem por custo/benefício:

1. **E — TargetFramework ausente.** É build quebrado. `/consertar-build`.
2. **C — contadores.** Edição de título, risco zero, sem tocar em código.
3. **A — projetos fora do índice.** Adicione `- \`Nome\` - descrição curta` na categoria certa,
   ordenação alfabética local, e ajuste o contador do título junto.
4. **B — entradas órfãs.** Decida com o usuário: renomear a entrada, remover, ou criar o `.csproj`
   que falta (caso `Sockets`).
5. **D — fora da `.sln`.** `dotnet sln CSharp-Codebase.sln add <caminho>` — o script já imprime os
   comandos prontos.
6. **F — documentação.** `/padronizar-readme` e `/atualizar-claude-md`, projeto a projeto.
7. **G — `var`.** Por último e por projeto, revisando cada ocorrência.

Nunca aplique 1-7 de uma vez sem combinar: são ~145 arquivos.

## Relato

Números da rodada, delta contra o baseline acima, e o que foi corrigido versus o que ficou como
dívida conhecida. Se nada mudou em relação ao baseline, diga isso — é o resultado esperado.
