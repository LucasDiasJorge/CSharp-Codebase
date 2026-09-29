# PropertyBasedTestingDemo

Biblioteca e suíte xUnit que geram entradas automaticamente, expressam invariantes em vez de exemplos, e medem o que o shrinking entrega — e o que ele não promete.

## Visão geral

Teste por exemplo responde "para esta entrada, espero esta saída". Teste por propriedade responde "para **qualquer** entrada do domínio, isto tem de continuar verdadeiro". A diferença aparece em quem escolhe as entradas: no primeiro caso, quem escreveu o código; no segundo, um gerador que não tem as mesmas suposições.

O exemplo usa dois assuntos com bug realista. **Dividir dinheiro**: a implementação ingênua arredonda cada parte e perde centavos. Ela passa em todos os exemplos que alguém escreveria à mão — 100/4, 10/2, 12/3, 99,99/3 — e a propriedade "a soma das partes é igual ao total" a reprova em milissegundos. **Compressão RLE**: `"aaab"` vira `"a3b1"`, e a volta funciona perfeitamente até o texto conter um dígito. A propriedade de ida-e-volta encontra o caso mínimo: **`"01"`**, dois caracteres.

A terceira parte é sobre o shrinking, e é onde este exemplo se afasta do discurso habitual. Shrinking **não** garante o menor contraexemplo. Com espaço de busca de mil valores e uma fronteira única, ele para exatamente em `900`, três execuções de três. Com o mesmo defeito em um espaço de um milhão, ele para em `989`, `945`, `931` — perto, nunca em cima. E no bug do dinheiro, que tem infinitos mínimos locais, cada execução devolve um par diferente: `(0.3, 11)`, `(1.5, 12)`, `(6.91, 11)`. O que ele garante é redução de ordens de grandeza e um caso legível, dentro do domínio do gerador.

## Conceitos abordados

- Propriedade (invariante) contra exemplo escolhido à mão.
- Geradores e o fato de que o gerador **é** parte do teste.
- Propriedade de conservação e de equidade.
- Propriedade de ida-e-volta (round-trip) em codificação.
- Propriedade metamórfica: relação entre dois resultados, sem oráculo.
- Teste diferencial: uma implementação como oráculo da outra.
- Shrinking, seu valor real e seus limites medidos.
- Gerador fora do domínio reprovando código correto.

## Objetivos de aprendizagem

- Enunciar o requisito como invariante antes de escrever exemplos.
- Escolher o gerador de modo que ele descreva o domínio, e não mais do que ele.
- Reconhecer as quatro famílias de propriedade mais úteis no dia a dia.
- Interpretar um contraexemplo encolhido sem esperar dele mais do que ele oferece.
- Distinguir "a propriedade falhou porque o código está errado" de "porque o gerador está errado".

## Estrutura do projeto

```text
PropertyBasedTestingDemo/
|-- Encoding/
|   `-- RunLengthEncoder.cs
|-- Money/
|   `-- MoneySplitter.cs
|-- PropertyBasedTestingDemo.Tests/
|   |-- Generators.cs
|   |-- MoneySplitterPropertyTests.cs
|   |-- PropertyFailure.cs
|   |-- RunLengthPropertyTests.cs
|   |-- ShrinkingTests.cs
|   `-- PropertyBasedTestingDemo.Tests.csproj
|-- PropertyBasedTestingDemo.csproj
`-- README.md
```

## Como executar

```bash
dotnet test 12-Testing/PropertyBasedTestingDemo/PropertyBasedTestingDemo.Tests/PropertyBasedTestingDemo.Tests.csproj
```

Para ver os contraexemplos que cada propriedade encontrou:

```bash
dotnet test 12-Testing/PropertyBasedTestingDemo/PropertyBasedTestingDemo.Tests/PropertyBasedTestingDemo.Tests.csproj --logger "console;verbosity=detailed"
```

Para validar apenas a compilação da biblioteca:

```bash
dotnet build 12-Testing/PropertyBasedTestingDemo/PropertyBasedTestingDemo.csproj
```

Não exige serviço externo. São 21 testes, e todos passam — inclusive os que **capturam** a falha de uma propriedade contra a implementação errada, em vez de deixar a suíte vermelha.

## Boas práticas e pontos de atenção

- Comece pelo requisito, não pelo código. "A soma das partes é o total" é propriedade; "100 dividido por 4 dá 25" é exemplo do mesmo requisito, e é o que o gerador vai encontrar sozinho.
- **O gerador é parte do teste.** Gerar `decimal` arbitrário para representar dinheiro reprova a implementação correta, porque dinheiro não tem fração de centavo. O teste `Gerador_ForaDoDominio_ReprovaAImplementacaoCORRETA` existe para mostrar esse erro de leitura.
- Restrinja o alfabeto quando o caso interessante for raro. O gerador de texto aqui usa `'0'..'c'`, com dígitos — em um alfabeto Unicode completo, o caso que quebra a RLE quase nunca apareceria.
- Não abandone teste por exemplo. Exemplo documenta intenção e serve de regressão nomeada; propriedade cobre o espaço. As duas coisas convivem nesta suíte.
- Prefira propriedades de conservação, ida-e-volta, idempotência e comparação com oráculo. São as que rendem mais defeito por linha escrita.
- Use propriedade metamórfica quando não souber calcular a resposta esperada. "Concatenar não aumenta o tamanho codificado" não exige conhecer o tamanho certo.
- Fixe a semente ao investigar, não na suíte. Semente fixa em CI transforma teste por propriedade em teste por exemplo com passos extras.
- Conte o custo: cada propriedade aqui roda 10.000 iterações. É barato para função pura e caro para o que toca I/O — property test não é teste de integração.
- Cuidado com propriedade que repete a implementação. Se a propriedade reimplementa o algoritmo para conferir o resultado, ela só testa se o desenvolvedor escreveu o mesmo bug duas vezes.
- Propriedade que falha e não encolhe é pouco útil na prática. Se o contraexemplo vem enorme, normalmente o gerador está mal escolhido.

## Conteúdo complementar

**1. Exemplo contra propriedade** — o mesmo requisito, dois resultados:

| Entrada | Ingênua | Soma | Correta | Soma |
|---|---|---|---|---|
| 100,00 / 4 | 25,00 × 4 | 100,00 ✓ | 25,00 × 4 | 100,00 ✓ |
| 10,00 / 2 | 5,00 × 2 | 10,00 ✓ | 5,00 × 2 | 10,00 ✓ |
| **0,01 / 2** | **0,00 + 0,00** | **0,00** | **0,01 + 0,00** | **0,01** |

Os exemplos escolhidos à mão passam nas duas implementações. A propriedade reprova uma delas.

**2. As famílias de propriedade usadas aqui**:

| Família | Enunciado no exemplo |
|---|---|
| Conservação | a soma das partes é igual ao total |
| Equidade | nenhuma parte difere de outra em mais de um centavo |
| Ida-e-volta | `Decode(Encode(texto)) == texto` |
| Metamórfica | `Encode(a + b).Length <= Encode(a).Length + Encode(b).Length` |
| Diferencial | a versão segura serve de oráculo para a ingênua |

**3. O contraexemplo da RLE**:

```text
contraexemplo minimo: 01   (2 caracteres, 6 a 8 encolhimentos)
```

Dois caracteres, um deles dígito. `"01"` codifica para `"0111"` — run de `'0'` com contagem 1, run de `'1'` com contagem 1 — e a volta lê `"111"` como contagem: `'0'` repetido 111 vezes. Nenhum tutorial de RLE inclui esse exemplo, porque quem escreve o exemplo está pensando em `"aaabbc"`.

**4. O que o shrinking entrega, medido**:

| Cenário | Execuções | Resultado |
|---|---|---|
| Fronteira única, espaço de 1.000 | 3 | **900, 900, 900** — em cima da fronteira |
| Fronteira única, espaço de 1.000.000 | 3 | **989, 945, 931** — perto, nunca em cima |
| Muitos mínimos (bug do dinheiro) | 3 | **(0.3, 11), (1.5, 12), (6.91, 11)** — sempre diferente |

A leitura correta: shrinking reduz de ordens de grandeza e entrega um caso legível **dentro do domínio do gerador**. Ele não é um minimizador exato, e depender de um valor específico torna o teste instável — foi o que aconteceu na primeira versão desta suíte, que afirmava `(0.01, 2)` como contraexemplo canônico e falhava a cada execução.

**5. Quando a propriedade acusa o teste, não o código**:

```text
contraexemplo: (760m/3, 2)
```

`760/3` não é um valor monetário: tem infinitas casas decimais. A implementação correta arredonda para centavos, a soma não fecha, e a propriedade reprova — **código certo, gerador errado**. Saber distinguir os dois casos é metade do trabalho de usar a técnica.

Relação com os vizinhos: `TestDoublesDemo` trata de com o que substituir colaboradores; aqui o assunto é o que verificar. `MutationTestingDemo` fecha o ciclo, medindo se a suíte realmente verifica algo.

## Referências e documentação complementar

- https://github.com/AnthonyLloyd/CsCheck
- https://fsharpforfunandprofit.com/posts/property-based-testing-2/
- https://hypothesis.works/articles/what-is-property-based-testing/
- https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test
