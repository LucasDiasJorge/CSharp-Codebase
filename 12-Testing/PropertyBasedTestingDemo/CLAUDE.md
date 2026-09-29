# CLAUDE.md — PropertyBasedTestingDemo

Biblioteca com dois assuntos (divisão de dinheiro e RLE), cada um em versão errada e correta, mais suíte xUnit + CsCheck com propriedades e medição de shrinking. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet test 12-Testing/PropertyBasedTestingDemo/PropertyBasedTestingDemo.Tests/PropertyBasedTestingDemo.Tests.csproj

# Ver os contraexemplos encontrados (o ITestOutputHelper so aparece em verbosity detailed)
dotnet test 12-Testing/PropertyBasedTestingDemo/PropertyBasedTestingDemo.Tests/PropertyBasedTestingDemo.Tests.csproj --logger "console;verbosity=detailed"
```

21 testes, todos passando. Sem serviço externo.

## Estrutura interna

`Money/MoneySplitter` e `Encoding/RunLengthEncoder` têm **cada um duas implementações**, a ingênua e a correta. As ingênuas não são código morto: são o sujeito dos testes que mostram a propriedade reprovando. **Não "consertar" nem remover `SplitEvenlyNaive`/`EncodeNaive`.**

`PropertyBasedTestingDemo.Tests/PropertyFailure` captura a `CsCheckException` e extrai o contraexemplo (segunda linha da mensagem) e a contagem de encolhimentos. É o que permite a suíte ficar **verde** enquanto demonstra propriedades que reprovam — sem ele, seria preciso deixar testes vermelhos de propósito.

`Generators` concentra os geradores. O alfabeto `Gen.Char['0', 'c']` inclui dígitos **de propósito** — é o que faz o bug da RLE aparecer em milhares de iterações. O total monetário é gerado em centavos (`Gen.Int[...].Select(cents => cents / 100m)`), porque gerar `decimal` arbitrário reprovaria a implementação correta.

`ShrinkingTests` tem três testes que medem comportamento real do CsCheck, não afirmações de folheto.

## Pontos de atenção

- TFM `net10.0` nos dois projetos (net9.0 compila mas não roda nesta máquina). Pacote de teste extra: `CsCheck` 4.9.1.
- **O projeto de testes fica aninhado na pasta da biblioteca**, então o `.csproj` da biblioteca precisa do `<Compile Remove="PropertyBasedTestingDemo.Tests\**\*.cs" />`. Sem isso, a biblioteca tenta compilar os testes e falha com `CS0246` sobre `Fact`.
- **Duas afirmações minhas sobre shrinking foram refutadas pela execução e reescritas:**
  1. A suíte afirmava `(0.01M, 2)` como contraexemplo canônico do bug do dinheiro. **Falso** — foram observados `(5.22, 8)`, `(8.79, 7)`, `(10.92, 10)`, `(52.85, 10)`, `(0.3, 11)`, `(1.5, 12)`, `(6.91, 11)`. O defeito tem infinitos mínimos locais.
  2. Depois disso a suíte afirmava que shrinking "converge para o mesmo contraexemplo quando há fronteira única". **Só vale se o espaço de busca for pequeno**: em `Gen.Int[0, 1_000]` dá 900 sempre; em `Gen.Int[0, 1_000_000]` dá 989/945/931.
  As asserções atuais usam `Assert.InRange` com folga, e o README registra as duas correções. **Não trocar por igualdade exata** — volta a falhar de forma intermitente.
- As asserções sobre contraexemplo de propriedade são **intrinsecamente frágeis**. Onde foi possível, o ponto é demonstrado também de forma determinística (`OMenorCasoAbsoluto_EOEnunciadoDoBug` e o trecho com `1.005m` em `Gerador_ForaDoDominio...`). Preferir esse padrão ao acrescentar cenários.
- O contraexemplo da RLE é estável em **formato** (2 caracteres, um dígito), não em valor: já saiu `01`, `20`, `00`. As asserções verificam o formato.
- `Assert.Contains("M", ...)` sobre a mensagem do CsCheck **não funciona**: ele imprime `decimal` como `760m/3`, com `m` minúsculo, e às vezes como fração. Não asseverar formato de literal decimal.
- Cada propriedade roda 10.000 iterações; a suíte inteira leva ~2s. Subir `iter` multiplica o tempo sem melhorar o exemplo.
- **Fronteira com os vizinhos**: `TestDoublesDemo` cobre dublês; `MutationTestingDemo` cobre a força da suíte. Não adicionar aqui geração de dados para teste de integração.
