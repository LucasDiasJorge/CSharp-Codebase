# CLAUDE.md — MutationTestingDemo

Regra de preço com duas suítes de mesma cobertura e forças opostas, medidor de escore de mutação próprio e Stryker.NET como ferramenta local. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
# O medidor proprio (nao depende de ferramenta externa)
dotnet test 12-Testing/MutationTestingDemo/MutationTestingDemo.Tests/MutationTestingDemo.Tests.csproj --logger "console;verbosity=detailed"

# Stryker de verdade — precisa de cd, porque o manifesto e local
cd 12-Testing/MutationTestingDemo
dotnet tool restore
dotnet stryker --project MutationTestingDemo.csproj \
               --test-project MutationTestingDemo.Tests/MutationTestingDemo.Tests.csproj \
               --mutate "Pricing/PricingRules.cs" --reporter cleartext
```

4 testes, todos passando. Stryker leva ~40s.

## Estrutura interna

`Pricing/PricingRules` é o sujeito: sete constantes e três fronteiras numéricas, escolhidas para render mutante de cada operador clássico.

`Pricing/Mutants` é o catálogo escrito à mão — sete variações com **uma** alteração cada, produzidas por uma única classe `MutatedRules` com pontos de mutação parametrizados. Existe para o exemplo rodar sem ferramenta externa e dar sempre o mesmo número.

`MutationTestingDemo.Tests/Suites` guarda as verificações como **dados** (`Check(nome, Action<IPricingRules>)`). É o que permite rodá-las duas vezes: contra a regra correta (são testes de verdade) e contra cada mutante (é a medição). **Não converter em `[Fact]` soltos** — isso elimina a medição.

`MutationScoreTests.Kills` implementa a regra da ferramenta: mutante morre se **alguma** verificação lançar.

## Pontos de atenção

- TFM `net10.0` nos dois projetos. O projeto de testes aninhado exige `<Compile Remove="MutationTestingDemo.Tests\**\*.cs" />` no `.csproj` da biblioteca.
- **`WeakSuite` tem escore 0%, e isso é o ponto.** Não "melhorar" as asserções dela; ela existe para ser fraca. O teste `SuiteFraca_TemCoberturaAltaEEscoreDeMutacaoBaixo` exige escore abaixo de 50%.
- **Episódio a preservar no README:** a `StrongSuite` começou com 86%, sobrevivendo o mutante `FreteFronteiraEstrita`, porque nenhum caso caía em exatamente 300 **após** o desconto. A verificação acrescentada usa subtotal **315,79** (solução de `S − round(S × 0,05, 2) = 300`). Mexer nas constantes de `PricingRules` invalida esse valor — recalcular, não apagar o caso.
- **O Stryker muta `Pricing/Mutants.cs` também** se não houver `--mutate`: 73 mutantes e escore de 56,52%, número sem significado. Sempre passar `--mutate "Pricing/PricingRules.cs"`.
- Escopado ao arquivo da regra, o Stryker gera **17 mutantes** e esta suíte mata todos (100%). Dois outros ficam `Ignored` pelo filtro de bloco já coberto.
- O manifesto está em `.config/dotnet-tools.json` (o `dotnet new tool-manifest --force` criou na raiz do projeto primeiro; foi movido). `dotnet stryker` só resolve a partir da pasta do projeto — daí o `cd` no comando.
- `StrykerOutput/` é gerado a cada execução e está no `.gitignore` local.
- **Fronteira com os vizinhos**: `TestDoublesDemo` e `PropertyBasedTestingDemo` tratam de escrever teste; aqui se mede a suíte. Não adicionar cobertura de código (coverlet já está nos projetos de teste do repositório).
