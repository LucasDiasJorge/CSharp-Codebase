---
name: atualizar-claude-md
description: Escreve ou atualiza o CLAUDE.md de um projeto do CSharp-Codebase no formato da casa — comando exato com o caminho real do .csproj, arquitetura interna explicando onde o conceito vive, e armadilhas específicas (TFM, build quebrado, serviço externo, warning conhecido). Use quando o pedido for criar, atualizar, revisar ou corrigir o CLAUDE.md de um projeto, ou quando um projeto novo ainda não tiver o seu.
---

# CLAUDE.md de projeto

Cada um dos 125 projetos tem um `CLAUDE.md` ao lado do `.csproj`, carregado automaticamente ao
trabalhar naquele diretório. Ele existe para responder o que o README não responde: **como não
perder tempo neste projeto específico.**

## O que ele não é

Não é README, não é tutorial e não é resumo do código.

| Pergunta | Onde responder |
|---|---|
| O que este exemplo ensina? Como estudo? | `README.md` |
| Qual comando roda? Onde o conceito vive? O que quebra? | `CLAUDE.md` |
| Regra do repositório inteiro (sem `var`, `readonly`, build) | `CLAUDE.md` **raiz** — não repita aqui |

Repetir as regras globais em cada projeto é o erro mais comum: elas já estão carregadas.
O `CLAUDE.md` local só carrega o que é **específico deste projeto**.

## Formato

Quatro blocos, nesta ordem. Modelo real (`06-Caching/Caching/CacheAside/CLAUDE.md`):

~~~markdown
# CLAUDE.md — NomeDoProjeto

Uma frase dizendo o que o projeto é. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build <caminho-real-do-csproj>
dotnet run --project <caminho-real-do-csproj>
```

## Estrutura interna

Onde o conceito ensinado realmente vive, arquivo por arquivo.

## Pontos de atenção

Armadilhas específicas deste projeto.
~~~

O link `[CLAUDE.md](...)` da abertura precisa de `../` na profundidade certa: dois níveis para
`01-Fundamentals/Projeto/`, três para `06-Caching/Caching/Projeto/`, quatro para
`13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk/`. Conte a partir da pasta do `.csproj`.

## Comandos

Caminho real, a partir da raiz do repositório. Confirme:

```bash
find <trilha>/<Projeto> -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*"
```

Inclua `dotnet test` quando houver projeto de testes associado, e o `docker compose up -d`
quando o sample exigir serviço externo. Mencione arquivos de apoio se existirem
(`Requisições de exemplo em CacheAside.http`).

## Estrutura interna — a seção que dá valor

Não liste arquivos. Explique **onde o conceito mora** e por que a divisão é essa. Um leitor deve
saber, ao terminar o parágrafo, qual arquivo abrir para mexer no que interessa.

O modelo bom é assim (de `CacheAside`):

> O padrão exige que **a aplicação**, e não a infraestrutura, decida sobre o cache. A separação
> reflete isso:
> - `Services/ProductService.cs` — **onde o padrão vive**: consulta o cache, em caso de miss vai
>   ao repositório, popula o cache e devolve. Nem o repositório nem o cache conhecem essa lógica.

Note o que ele faz: dá a razão da estrutura, marca o arquivo central em negrito, e diz o que
cada peça **não** sabe. Um `ls` comentado não faria isso.

Se o projeto tiver convenção de extensão — "adicione um arquivo em `Demos/` e registre no
despacho do `Program.cs`; não infle os demos existentes" —, diga. É o que impede que a próxima
edição estrague a organização.

## Pontos de atenção — só o que surpreende

Inclua apenas o que faria alguém perder tempo. Candidatos reais:

- **TFM fora do padrão da trilha.** `net10.0` numa trilha majoritariamente `net9.0`; `net5.0`,
  `net6.0` ou `net7.0` (fora de suporte). Sempre com a instrução de **preservar**.
- **Build quebrado**, com a causa e o alvo correto ao consertar. Ver a tabela em
  [`.claude/reference/catalogo.md`](../../reference/catalogo.md).
- **Serviço externo obrigatório**, e a distinção quando o pacote está referenciado mas não é
  usado — `CacheAside` referencia Redis e roda sem ele; essa frase economiza um `docker run`.
- **Configuração frágil do `.csproj`** que uma edição descuidada quebraria — como o
  `<Compile Remove="OrderRuleConsole.Tests\**\*.cs" />` do `OrderRuleConsole`.
- **Warning conhecido**, com arquivo e linha, dizendo se é aceitável ou limpeza legítima.
- **Contraste com projeto vizinho** que ensina algo próximo — "`CachePatterns` compara oito
  estratégias; aqui uma é implementada a fundo". Ajuda a escolher onde mexer.
- **Ausência de dependências**, quando vale dizer: "sem dependências externas" encerra a dúvida.

Se o projeto for simples e não tiver nenhuma armadilha, a seção pode ter duas linhas. Não invente
conteúdo para preenchê-la.

## Procedimento

1. Localize o `.csproj` e leia o `.csproj` inteiro — TFM, pacotes, `ItemGroup` incomum.
2. Leia `Program.cs` e os arquivos que carregam o conceito; entenda a divisão antes de descrevê-la.
3. Leia o `README.md` local — o `CLAUDE.md` **complementa**, não duplica.
4. Rode `dotnet build <csproj>` e anote o resultado real: warnings viram *Pontos de atenção*;
   falha vira a primeira linha da seção, com a causa.
5. Escreva os quatro blocos. Português-BR, direto, sem ornamentação.
6. Confira o link `../CLAUDE.md` e os caminhos dos comandos.

Ao **atualizar** um existente: leia o atual primeiro e preserve as observações ainda válidas.
Alguém já pagou o custo de descobrir aquelas armadilhas — corrija o que envelheceu, não recomece.

## Verificação

- [ ] Título `# CLAUDE.md — NomeDoProjeto`.
- [ ] Link para o `CLAUDE.md` raiz com a quantidade certa de `../`.
- [ ] Comandos com o caminho real do `.csproj`, testados.
- [ ] *Estrutura interna* diz **onde o conceito vive**, não só quais arquivos existem.
- [ ] *Pontos de atenção* só com o que é específico deste projeto.
- [ ] Nenhuma regra global repetida (sem `var`, `readonly`, PascalCase, "validar com build").
