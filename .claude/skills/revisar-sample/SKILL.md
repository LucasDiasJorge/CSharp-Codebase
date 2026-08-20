---
name: revisar-sample
description: Revisa um projeto do CSharp-Codebase contra as convenções obrigatórias do repositório — proibição de var, PascalCase/camelCase, readonly em dependências injetadas, ILogger, foco didático, TFM preservado, README completo, CLAUDE.md presente, registro na .sln e build do csproj alvo. Use quando o pedido for revisar, auditar, verificar convenções, checar estilo ou validar um sample/projeto existente.
---

# Revisar sample

Auditoria de conformidade de **um** projeto contra as regras do repositório. Para uma varredura
de todo o catálogo, use `/auditar-catalogo`.

Referências: [`CLAUDE.md`](../../../CLAUDE.md) (regras),
[`.claude/reference/catalogo.md`](../../reference/catalogo.md) (dados),
[`.github/instructions/csharp-style.instructions.md`](../../../.github/instructions/csharp-style.instructions.md).

## Escopo

Se o usuário não indicar o projeto, revise o que estiver no diff atual
(`git status --short`, `git diff --name-only`). Se o diff estiver vazio e nenhum projeto for
nomeado, pergunte qual revisar — não varra os 125.

Primeiro, localize o `.csproj` real (o layout varia):

```bash
find <trilha>/<Projeto> -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*"
```

## Verificações

Rode na ordem. Cada uma é barata até a última.

### 1. `var` proibido

Regra: nunca usar `var`, exceto quando um tipo anônimo de LINQ tornar o tipo explícito impossível.

```bash
grep -rn --include="*.cs" -E "(^|[^A-Za-z0-9_])var[[:space:]]+[a-zA-Z_]" <pasta-do-projeto> \
  | grep -v "/bin/" | grep -v "/obj/"
```

A heurística é ruidosa. **Revise cada ocorrência** e classifique:

| Situação | Veredito |
|---|---|
| `var x = new Foo();` | violação — trocar por `Foo x = new Foo();` |
| `var x = lista.Select(i => new { i.Id, i.Nome });` | **permitido** — tipo anônimo |
| `foreach (var item in lista)` | violação — usar o tipo do elemento |
| Palavra `var` dentro de comentário, string ou nome como `variavel` | falso positivo — ignorar |

Ao corrigir, escreva o tipo real, não `object`. Se o tipo for verboso
(`Dictionary<string, List<Pedido>>`), escreva assim mesmo — é o ponto didático da regra.

### 2. Nomenclatura

- Classes, métodos, propriedades e constantes públicas: **PascalCase**.
- Parâmetros e variáveis locais: **camelCase**.
- Campos privados: seguir o padrão já usado no projeto (`_campo` ou `campo`); não uniformize
  o repositório inteiro de passagem.

### 3. `readonly` em dependências injetadas

Todo campo atribuído no construtor a partir de um parâmetro de injeção deve ser `readonly`.

```bash
grep -rn --include="*.cs" -E "private[[:space:]]+(?!readonly)" -P <pasta-do-projeto>
```

Se `grep -P` não estiver disponível, inspecione os construtores diretamente.

### 4. Logs com `ILogger`

Em projetos com DI (Web API, worker, host genérico): logs estruturados via `ILogger<T>`, com
placeholders nomeados — `logger.LogInformation("Pedido {PedidoId} criado", pedidoId)`, nunca
interpolação de string dentro da mensagem.

`Console.WriteLine` é **legítimo** em samples console de `01-Fundamentals` e `10-Algorithms`,
onde a saída é o material didático. Não converta esses para `ILogger`.

### 5. Foco didático

O exemplo deve demonstrar um conceito. Sinalize como excesso: camadas de abstração sem uso,
interfaces com uma única implementação e nenhum ponto de variação didático, injeção de
dependência num console de 40 linhas, configuração externa que o exemplo nunca lê.

Isto é observação de revisão, não licença para refatorar — proponha, não reescreva, salvo pedido.

### 6. TFM preservado

```bash
grep "<TargetFramework" <caminho-do-csproj>
```

Se ausente, é o `NETSDK1013` conhecido (10 projetos) → `/consertar-build`. Se presente, **não
modernize** o TFM. Alterar `net6.0` para `net9.0` sem pedido é uma regressão de escopo.

### 7. Documentação

- `README.md` ao lado do `.csproj`, com as 9 seções na ordem de `docs/CONVENCOES.md`,
  sem placeholders do template, com caminho real do `.csproj` nos comandos, árvore sem
  `bin/`/`obj/`. Detalhes e correção: `/padronizar-readme`.
- `CLAUDE.md` ao lado do `.csproj`. Ausente ou desatualizado → `/atualizar-claude-md`.
- Projeto listado no índice do README raiz, na categoria certa, e contador da categoria coerente.

Projetos agrupados sob uma pasta-mãe (`05-Messaging/Kafka/Send`, `09-Data/Data/*`) podem ser
cobertos pelo README da pasta-mãe — verifique lá antes de reportar README ausente.

### 8. Build

```bash
dotnet build <caminho-do-csproj>
```

Critério de conclusão. Se falhar, compare com o baseline conhecido em
[`.claude/reference/catalogo.md`](../../reference/catalogo.md) antes de chamar de regressão.

Havendo projeto de testes associado:

```bash
dotnet test <caminho-do-tests-csproj>
```

## Relato

Agrupe por severidade e seja específico — arquivo e linha, no formato `caminho/arquivo.cs:42`:

- **Bloqueia** — build falha, `var` no código, TFM ausente, README com placeholder.
- **Corrigir** — nomenclatura, `readonly` faltando, log não estruturado, doc desatualizada.
- **Observação** — excesso de abstração, oportunidade didática, inconsistência menor.

Não invente achados para preencher categoria: se o projeto está conforme, diga isso e liste o
que foi verificado. Aplique correções apenas se o usuário pediu revisão **e** correção.
