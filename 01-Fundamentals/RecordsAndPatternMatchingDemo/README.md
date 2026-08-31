# RecordsAndPatternMatchingDemo

Projeto console didatico sobre `record`, igualdade por valor, expressoes `with` e pattern matching aplicado a dados imutaveis.

## Visao geral

Este exemplo modela clientes, enderecos, itens e pedidos usando records. Ao executar o programa, observe como dois records com os mesmos valores sao considerados iguais e como a expressao `with` cria uma nova instancia preservando os demais dados.

O projeto tambem demonstra pattern matching posicional, por propriedade e por lista para classificar pedidos sem espalhar condicionais longas pelo codigo.

## Conceitos abordados

- `record` e igualdade por valor.
- Expressao `with` para criar copias modificadas.
- Pattern matching posicional em records.
- Pattern matching por propriedade, incluindo propriedades aninhadas.
- Pattern matching por lista em arrays de itens.

## Objetivos de aprendizagem

- Entender por que records combinam bem com modelos de dados imutaveis.
- Comparar igualdade por valor com reaproveitamento de referencias.
- Aplicar `with` para representar mudancas de estado sem alterar o objeto original.
- Usar patterns para tornar regras de classificacao mais expressivas.

## Estrutura do projeto

```text
RecordsAndPatternMatchingDemo/
|-- Program.cs
|-- RecordsAndPatternMatchingDemo.csproj
`-- README.md
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/RecordsAndPatternMatchingDemo/RecordsAndPatternMatchingDemo.csproj
```

Para validar compilacao:

```bash
dotnet build 01-Fundamentals/RecordsAndPatternMatchingDemo/RecordsAndPatternMatchingDemo.csproj
```

## Boas praticas e pontos de atencao

- Records sao uma boa escolha quando identidade estrutural e igualdade por valor sao mais importantes que identidade por referencia.
- A expressao `with` faz uma copia nao destrutiva do record, mas referencias internas continuam sendo compartilhadas.
- Para imutabilidade mais forte em colecoes, prefira tipos imutaveis ou exponha colecoes somente leitura.
- Pattern matching deve comunicar regras de negocio; evite patterns tao densos que fiquem mais dificeis que um `if` simples.

## Conteudo complementar

O sample usa `OrderLine[]` para mostrar list patterns com sintaxe direta. Em codigo de producao, avalie `IReadOnlyList<T>`, `ImmutableArray<T>` ou outra colecao que deixe a intencao de imutabilidade mais clara.

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record
- https://learn.microsoft.com/dotnet/csharp/language-reference/operators/with-expression
- https://learn.microsoft.com/dotnet/csharp/fundamentals/functional/pattern-matching
