---
name: padronizar-readme
description: Escreve ou normaliza o README local de um projeto do CSharp-Codebase segundo docs/CONVENCOES.md e docs/README_TEMPLATE.md — 9 seções na ordem fixa, Português-BR, árvore curta sem bin/obj, comandos com o caminho real do .csproj e nenhum placeholder do template. Use quando o pedido for criar, padronizar, corrigir, revisar ou atualizar o README/documentação de um projeto ou pasta didática.
---

# Padronizar README

Fontes de verdade: [`docs/CONVENCOES.md`](../../../docs/CONVENCOES.md) e
[`docs/README_TEMPLATE.md`](../../../docs/README_TEMPLATE.md). Leia os dois antes de escrever —
esta skill traz o procedimento, não substitui o template.

Para o índice do README **raiz**, use `/auditar-catalogo`. Aqui o alvo é o README **local**.

## Regra que mais se quebra

**Reorganize antes de reescrever.** Se o README existente tem conteúdo útil — tabela de
endpoints, fluxo de negócio, comparação entre abordagens, exercícios — esse material é
preservado e realocado para *Conteúdo complementar*. Reescrever do zero apaga o trabalho de
estudo que dá valor ao sample. Leia o README atual inteiro antes de tocar nele.

## Ordem fixa das seções

Exatamente esta, sem inventar nem reordenar:

1. **Título** — nome do projeto.
2. **Visão geral** — 1 a 2 parágrafos: objetivo didático, contexto, o que observar ao executar.
3. **Conceitos abordados** — lista curta dos conceitos técnicos.
4. **Objetivos de aprendizagem** — o que o leitor sabe fazer depois.
5. **Estrutura do projeto** — árvore curta em bloco ```text.
6. **Como executar** — comandos reais.
7. **Boas práticas e pontos de atenção** — decisões técnicas, pré-requisitos, limitações.
8. **Conteúdo complementar** — *opcional*: tabelas, fluxos, variações, exercícios.
9. **Referências e documentação complementar** — *opcional*: links e docs vizinhos.

Seções 8 e 9 são omitidas quando não há conteúdo — melhor ausentes que vazias. As 7 primeiras
são obrigatórias.

## Estilo

- Português-BR; termos técnicos em inglês quando mais reconhecíveis (`middleware`, `cache`,
  `background worker`). Não traduza à força.
- Texto direto e didático, sem ornamentação. Pouco emoji, sem numeração manual de seções.
- Mantenha o idioma do arquivo que está editando.
- Encoding UTF-8. Se o README existente tem mojibake (`Ã§`, `Ã£`), corrija o texto afetado.

## Árvore de estrutura

Curta e real. Omita `bin/`, `obj/`, `.git/`, `.vs/`. Use o estilo `|--` / `` `-- `` do template:

```text
YieldReturnDemo/
|-- Demos/
|   |-- BasicsDemo.cs
|   `-- PitfallsDemo.cs
|-- LazyOperators.cs
|-- Program.cs
`-- YieldReturnDemo.csproj
```

Em projeto grande, mostre as pastas e só os arquivos que importam para o estudo. A árvore é mapa,
não `ls -R`.

## Comandos

Sempre o caminho **real** do `.csproj`, a partir da raiz do repositório. Confirme antes:

```bash
find <trilha>/<Projeto> -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*"
```

| Tipo de projeto | Comando |
|---|---|
| Executável (console, API, worker) | `dotnet run --project <caminho>.csproj` |
| Biblioteca / sem entrypoint | `dotnet build <caminho>.csproj` |
| Projeto de testes | `dotnet test <caminho>.csproj` |
| Pasta com vários samples | um bloco por subprojeto, cada um com seu caminho |

Se o projeto exigir serviço externo, o comando de subida (`docker run` ou `docker compose up -d`)
vem **antes**, em *Como executar*, e o pré-requisito é repetido em *Boas práticas e pontos de
atenção*. Consulte a matriz em [`.claude/reference/catalogo.md`](../../reference/catalogo.md).

## Placeholders — erro mais comum

Nenhum texto do template pode sobreviver. Antes de terminar:

```bash
grep -nE "Título do Projeto|NomeDoProjeto|NomeDaPasta|Conceito principal [0-9]|Objetivo de estudo [0-9]|Link de referência|caminho/para" <README>
```

Zero resultados. `Subpasta/`, `Destaque as decisões` e `Descreva em um ou dois parágrafos`
também são placeholders.

## Lote

Para padronizar muitos READMEs de uma vez existe o utilitário do repositório:

```bash
powershell -ExecutionPolicy Bypass -File .\tools\Standardize-Readmes.ps1
```

Ele normaliza estrutura, não conteúdo — revise o resultado. Leia
`tools/ReadmeStandardizer/CLAUDE.md` antes de rodar, e inspecione o diff depois: um script não
distingue conteúdo útil de placeholder.

## Verificação final

- [ ] 7 seções obrigatórias presentes, na ordem, sem seções inventadas.
- [ ] Zero placeholders (grep acima limpo).
- [ ] Árvore reflete o disco e omite `bin/`/`obj/`.
- [ ] Comandos usam o caminho real do `.csproj` e o verbo certo (`run`/`build`/`test`).
- [ ] Pré-requisitos externos declarados.
- [ ] Conteúdo útil do README anterior preservado.
- [ ] Links relativos resolvem.
