# MutationTestingDemo

Biblioteca de regra de preço com duas suítes de mesma cobertura e forças opostas, um medidor de escore de mutação que roda sem ferramenta externa, e o Stryker.NET configurado para a medição real.

## Visão geral

Cobertura responde "esta linha foi executada?". Mutation testing responde a pergunta que importa: **"se esta linha estivesse errada, algum teste reclamaria?"**. A diferença entre as duas perguntas é o assunto deste exemplo, e ela é medida.

A suíte fraca passa por todas as faixas de desconto, pelas duas pernas do frete e pelo caminho do cliente fidelidade — cobertura alta. E afirma coisas sem conteúdo: que não lança, que o total é positivo, que o desconto não é negativo, que o frete é 0 ou 25. Resultado contra sete mutantes: **0% de escore de mutação**. Ela não mata **nenhum**. Nem o mutante que remove o desconto inteiro, nem o que inverte o sinal no total.

A suíte forte tem a mesma cobertura e fixa valores exatos nas fronteiras. Mata **7 de 7**.

O caminho até esses 100% é a parte mais instrutiva, e aconteceu de verdade aqui: a suíte forte começou com **86%**, e o sobrevivente foi o mutante que troca `>= 300` por `> 300` no frete grátis. O motivo era um furo real — nenhum teste caía em exatamente 300 **depois** do desconto, porque o desconto move o valor. Achar a entrada exigiu resolver `S - round(S × 5%, 2) == 300`, que dá **S = 315,79**. O mutante sobrevivente não apontou um teste faltando em abstrato: apontou uma conta que ninguém tinha feito.

## Conceitos abordados

- Escore de mutação e sua diferença em relação a cobertura.
- Mutante morto contra mutante sobrevivente.
- Operadores de mutação: fronteira de comparação, constante, remoção de condicional, inversão aritmética, retorno fixo.
- Teste que executa código sem verificar comportamento.
- Asserção em fronteira exata como o que mata mutante de limite.
- Stryker.NET como ferramenta local (`dotnet tool`) e o filtro `--mutate`.

## Objetivos de aprendizagem

- Calcular escore de mutação e interpretá-lo junto da cobertura.
- Reconhecer, olhando uma asserção, se ela mataria um mutante.
- Usar mutante sobrevivente como especificação do teste que falta.
- Rodar Stryker.NET em um projeto e restringir o escopo da mutação.

## Estrutura do projeto

```text
MutationTestingDemo/
|-- .config/
|   `-- dotnet-tools.json
|-- Pricing/
|   |-- Mutants.cs
|   `-- PricingRules.cs
|-- MutationTestingDemo.Tests/
|   |-- MutationScoreTests.cs
|   |-- Suites.cs
|   `-- MutationTestingDemo.Tests.csproj
|-- MutationTestingDemo.csproj
`-- README.md
```

## Como executar

O medidor próprio, que não depende de ferramenta externa:

```bash
dotnet test 12-Testing/MutationTestingDemo/MutationTestingDemo.Tests/MutationTestingDemo.Tests.csproj --logger "console;verbosity=detailed"
```

O Stryker.NET, restaurado como ferramenta local do projeto:

```bash
cd 12-Testing/MutationTestingDemo
dotnet tool restore
dotnet stryker --project MutationTestingDemo.csproj \
               --test-project MutationTestingDemo.Tests/MutationTestingDemo.Tests.csproj \
               --mutate "Pricing/PricingRules.cs" \
               --reporter cleartext
```

São 4 testes, todos passando, e a saída detalhada imprime os dois escores e a lista de sobreviventes. O Stryker leva cerca de 40s e grava relatório em `StrykerOutput/` (ignorado pelo Git).

## Boas práticas e pontos de atenção

- Não confunda cobertura com verificação. A suíte fraca daqui tem cobertura alta e escore **zero**: ela executa tudo e afirma nada.
- Desconfie de asserção que aceitaria muitos resultados. `Assert.True(total > 0)`, `Assert.NotNull`, "não lançou" e "está em uma lista de valores plausíveis" sobrevivem a quase qualquer mutação.
- Teste fronteiras **nos dois lados e no valor exato**. Mutante de `>=` para `>` só morre com um caso em cima do limite; é o operador mais comum e o mais sobrevivente.
- Use o mutante sobrevivente como especificação. Ele não diz "escreva mais testes", diz exatamente qual comportamento ninguém fixou.
- Restrinja o escopo antes de olhar o número. Sem `--mutate`, o Stryker mutou também o catálogo de mutantes escrito à mão deste projeto e o escore caiu para **56,52%** — um número sem significado, porque mutar um mutante não diz nada sobre a suíte.
- Nem todo sobrevivente merece um teste. Mutante equivalente (que não muda comportamento observável) e código sem regra de negócio produzem ruído; perseguir 100% cegamente gera teste acoplado a detalhe.
- Mutation testing é caro: cada mutante exige rodar a suíte. Rode por módulo crítico e em CI noturno, não a cada commit.
- Suíte lenta multiplica o custo por mutante. Mutation testing costuma ser o primeiro lugar onde o tempo da suíte deixa de ser um detalhe.
- Fixe a ferramenta como ferramenta local (`dotnet tool install` com manifesto). Ferramenta global torna o resultado dependente da máquina de quem rodou.

## Conteúdo complementar

**1. Os sete mutantes e o que cada suíte faz com eles**:

| Mutante | Alteração | Suíte fraca | Suíte forte |
|---|---|---|---|
| `FronteiraAltaEstrita` | `>= 500` → `> 500` | sobrevive | morre |
| `TaxaAltaTrocada` | 10% → 12% | sobrevive | morre |
| `FidelidadeIgnorada` | bônus de 2% nunca aplicado | sobrevive | morre |
| `FreteFronteiraEstrita` | `>= 300` → `> 300` | sobrevive | morre |
| `FreteSempreGratis` | frete sempre 0 | sobrevive | morre |
| `SinalDoDescontoInvertido` | `subtotal + desconto` | sobrevive | morre |
| `DescontoRemovido` | desconto sempre 0 | sobrevive | morre |
| **Escore** | | **0%** | **100%** |

As duas suítes passam contra a implementação correta. "Tudo verde" não distingue uma da outra.

**2. A asserção que mata, lado a lado**:

```csharp
// Suite fraca: sobrevive a QUALQUER mutacao que mantenha o total positivo.
Assert.True(rules.Calculate(cart).Total > 0m);

// Suite forte: em 500, o desconto e 50,00 e nada mais.
Assert.Equal(50m, rules.Calculate(new Cart(1, 500m, false)).Discount);
```

```text
subtotal 500 -> correto: 50,00 de desconto | mutante FronteiraAltaEstrita: 25,00
```

**3. O sobrevivente que virou teste** — o episódio real deste projeto:

```text
1a medicao da suite forte:  mortos 6/7 (86%)
  SOBREVIVEU: FreteFronteiraEstrita — >= 300 virou > 300 na fronteira do frete gratis
```

Havia dois testes de frete, e nenhum caía em 300 depois do desconto: `300 − 5% = 285` (abaixo) e `320 − 5% = 304` (acima). A entrada que atinge o limite exato sai de `S − round(S × 0,05, 2) = 300`:

```text
S = 315,79  ->  desconto 15,79  ->  total 300,00  ->  frete gratis
```

Com esse caso acrescentado, o escore foi para 100%.

**4. O Stryker.NET nos mesmos arquivos**:

```text
dotnet stryker --mutate "Pricing/PricingRules.cs"
  17 mutantes testados
  escore de mutacao: 100,00%
```

Operadores que ele gerou sozinho para as 25 linhas da regra:

| Operador | Mutantes |
|---|---:|
| Equality mutation (`>=` → `>`, `==` → `!=`) | 6 |
| Conditional (true) mutation | 3 |
| Conditional (false) mutation | 3 |
| Arithmetic mutation (`-` → `+`) | 3 |
| Block removal mutation | 2 |
| Negate expression | 1 |
| `+=` → `-=` | 1 |

Os sete mutantes escritos à mão neste projeto são um subconjunto legível do que a ferramenta produz automaticamente. **Sem** o filtro `--mutate`, o Stryker mutou também `Pricing/Mutants.cs` — 73 mutantes no total e escore de 56,52%, porque mutar o catálogo de mutantes não mede nada.

Relação com os vizinhos: `TestDoublesDemo` e `PropertyBasedTestingDemo` tratam de como escrever teste; este mede se o que foi escrito verifica algo. `BenchmarkTool` mede desempenho, não força de suíte.

## Referências e documentação complementar

- https://stryker-mutator.io/docs/stryker-net/introduction/
- https://stryker-mutator.io/docs/stryker-net/configuration/
- https://en.wikipedia.org/wiki/Mutation_testing
- https://learn.microsoft.com/dotnet/core/tools/global-tools#install-a-local-tool
