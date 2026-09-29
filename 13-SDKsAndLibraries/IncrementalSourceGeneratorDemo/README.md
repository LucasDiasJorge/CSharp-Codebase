# IncrementalSourceGeneratorDemo

Gerador de código incremental com Roslyn que produz `ToStringFast()` para enums marcados, um consumidor que lê e executa o código gerado, e uma suíte que verifica entradas, saídas, diagnósticos **e o cache incremental**.

## Visão geral

`Enum.ToString()` usa reflexão e aloca. Um `switch` sobre os membros conhecidos em tempo de compilação devolve uma constante de string. O gerador escreve esse `switch` para cada enum marcado com `[EnumExtensions]`, e o ganho é medido pelo consumidor:

```text
2.000.000 chamadas:
  ToString()     :  28ms,  45,8 MB alocados
  ToStringFast() :   2ms,   0,0 MB alocados
```

O exemplo tem três partes que se sustentam:

- **O gerador** (`netstandard2.0`, obrigatoriamente) injeta o próprio atributo por `RegisterPostInitializationOutput`, acha os enums com `ForAttributeWithMetadataName`, converte o símbolo em um modelo pequeno **com igualdade por valor**, e emite um arquivo por enum. Enum inacessível recebe o diagnóstico `AEE001` em vez de código que não compilaria.
- **O consumidor** referencia o gerador como *analisador* e grava o código gerado em disco (`EmitCompilerGeneratedFiles`), de forma que dá para ler o que foi produzido.
- **Os testes** rodam o gerador como uma função — entra código, sai código — e um deles verifica o que dá nome à interface: que o cache funciona.

Esse último ponto é o que distingue `IIncrementalGenerator` do antigo `ISourceGenerator`, e é verificável. Medido aqui: com entrada idêntica, `SourceOutput: Cached`. Com uma classe nova acrescentada **fora** do enum, a árvore muda e ainda assim `SourceOutput: Cached` — porque o modelo do enum não mudou. Com um membro novo **no** enum, `SourceOutput: Modified` e o arquivo é regerado.

## Conceitos abordados

- `IIncrementalGenerator` e a montagem de um pipeline.
- `ForAttributeWithMetadataName` como ponto de entrada barato.
- `RegisterPostInitializationOutput` para injetar o atributo marcador.
- Modelo com igualdade por valor como requisito do cache.
- Diagnóstico (`DiagnosticDescriptor`, `AnalyzerReleases.Unshipped.md`) em vez de código inválido.
- `netstandard2.0` como alvo obrigatório de analisador.
- `OutputItemType="Analyzer"` e `ReferenceOutputAssembly="false"`.
- `EmitCompilerGeneratedFiles` para inspecionar a saída.
- `CSharpGeneratorDriver` com `trackIncrementalGeneratorSteps` para testar o cache.

## Objetivos de aprendizagem

- Escrever um gerador incremental que não recalcula o que não mudou.
- Reconhecer o que **não** pode atravessar o pipeline (`ISymbol`, `SyntaxNode`, `ImmutableArray<T>` no modelo).
- Emitir diagnóstico útil em vez de gerar código quebrado.
- Ler o código gerado durante a depuração de um problema.
- Testar gerador sem projeto de apoio, inclusive a incrementalidade.

## Estrutura do projeto

```text
IncrementalSourceGeneratorDemo/
`-- src/
    |-- Acme.EnumExtensions.Generator/
    |   |-- AnalyzerReleases.Unshipped.md
    |   |-- EnumExtensionsGenerator.cs
    |   |-- EnumToGenerate.cs
    |   `-- Acme.EnumExtensions.Generator.csproj
    |-- Acme.EnumExtensions.Consumer/
    |   |-- Enums.cs
    |   |-- Program.cs
    |   `-- Acme.EnumExtensions.Consumer.csproj
    `-- Acme.EnumExtensions.Generator.Tests/
        |-- GenerationTests.cs
        |-- GeneratorHarness.cs
        |-- IncrementalityTests.cs
        `-- Acme.EnumExtensions.Generator.Tests.csproj
```

## Como executar

```bash
dotnet run -c Release --project 13-SDKsAndLibraries/IncrementalSourceGeneratorDemo/src/Acme.EnumExtensions.Consumer/Acme.EnumExtensions.Consumer.csproj
dotnet test 13-SDKsAndLibraries/IncrementalSourceGeneratorDemo/src/Acme.EnumExtensions.Generator.Tests/Acme.EnumExtensions.Generator.Tests.csproj
```

Com as razões de cache de cada passo do pipeline:

```bash
dotnet test 13-SDKsAndLibraries/IncrementalSourceGeneratorDemo/src/Acme.EnumExtensions.Generator.Tests/Acme.EnumExtensions.Generator.Tests.csproj --logger "console;verbosity=detailed"
```

Para **ler** o código gerado, depois de compilar o consumidor:

```text
src/Acme.EnumExtensions.Consumer/obj/generated/Acme.EnumExtensions.Generator/
  Acme.EnumExtensions.Generator.EnumExtensionsGenerator/
    EnumExtensionsAttribute.g.cs
    global.OrderStatus.EnumExtensions.g.cs
    global.Priority.EnumExtensions.g.cs
    Shipping.Carrier.EnumExtensions.g.cs
```

São 11 testes, todos passando. Não exige serviço externo.

## Boas práticas e pontos de atenção

- **O gerador precisa ser `netstandard2.0`.** O compilador carrega o analisador no processo dele, que pode ser qualquer versão do .NET. Um gerador em `net10.0` simplesmente não é carregado, e o sintoma é "não gera nada" — sem erro, sem aviso.
- Nunca deixe `ISymbol` ou `SyntaxNode` atravessar o pipeline. Eles carregam a árvore e o modelo semântico inteiros: guardá-los impede o cache de funcionar e segura memória entre compilações.
- Dê igualdade por valor ao modelo. E cuidado com `ImmutableArray<T>` dentro dele: a igualdade dele é **por referência**, o que derruba o cache sem quebrar nada visivelmente. Aqui a comparação usa `SequenceEqual` explicitamente.
- Prefira `ForAttributeWithMetadataName` a `CreateSyntaxProvider`. O compilador mantém um índice de atributos, e o predicado sintático nem é chamado para a maioria dos nós.
- Filtre cedo e transforme para o menor modelo possível. O trabalho caro tem de ficar depois do ponto em que o cache pode cortar.
- Emita diagnóstico em vez de código inválido. Código gerado que não compila aparece como erro no projeto de quem consome, apontando para um arquivo que a pessoa não escreveu.
- Registre as regras em `AnalyzerReleases.Unshipped.md`. Sem isso o build avisa `RS2008`, e o motivo é legítimo: quem consome precisa saber em que versão cada diagnóstico apareceu.
- Use `hintName` único por tipo. Dois enums com o mesmo nome em namespaces diferentes colidem se o nome do arquivo só usar o nome do tipo — há um teste fixando isso.
- Qualifique tudo com `global::` no código gerado. O arquivo gerado não sabe quais `using` existem onde ele será compilado, e um nome ambíguo quebra apenas no projeto de alguém.
- A extensão gerada fica no namespace do **enum**, não do gerador. Quem chama precisa daquele `using` — foi um `CS1929` real ao escrever este exemplo.
- Ligue `EmitCompilerGeneratedFiles` durante o desenvolvimento. Depurar gerador sem ver a saída é adivinhação.
- Teste o cache, não só a saída. Um gerador correto que invalida o cache a cada tecla digitada trava a IDE de quem usa.

## Conteúdo complementar

**1. O pipeline, em três etapas**:

```text
RegisterPostInitializationOutput  ->  injeta EnumExtensionsAttribute (roda uma vez)
         |
ForAttributeWithMetadataName      ->  acha os enums marcados (indice de atributos)
         |
         Transform: ISymbol -> EnumToGenerate       <- fronteira do cache
         |                     (igualdade por VALOR)
RegisterSourceOutput              ->  um arquivo por enum
```

**2. O que sai do gerador**, arquivo real gerado para `Priority`:

```csharp
// <auto-generated/>
#nullable enable

internal static class PriorityExtensions
{
    /// <summary>Devolve o nome do valor sem reflexao e sem alocar.</summary>
    public static string ToStringFast(this Priority value)
        => value switch
        {
            Priority.Low => nameof(Priority.Low),
            Priority.Medium => nameof(Priority.Medium),
            Priority.High => nameof(Priority.High),
            _ => value.ToString(),
        };
    // ... DeclaredValues
}
```

O enum `internal` gerou extensão `internal`; enum `public` gera `public`. Há teste para os dois.

**3. O ganho, medido pelo consumidor** — 2 milhões de chamadas:

| Chamada | Tempo | Alocado |
|---|---:|---:|
| `ToString()` | 28ms | **45,8 MB** |
| `ToStringFast()` | **2ms** | **0,0 MB** |

Os 45,8 MB são uma string nova por chamada. O `switch` gerado devolve a constante literal — o mesmo objeto, sempre.

**4. O cache, medido** (razão do passo `SourceOutput` na segunda execução):

| Segunda execução | `SourceOutput` |
|---|---|
| Entrada idêntica | **Cached** |
| Classe nova acrescentada **fora** do enum | **Cached** |
| Membro novo **no** enum | **Modified** |

O caso do meio é o que justifica o trabalho de dar igualdade por valor ao modelo: a árvore de sintaxe mudou (`compilationAndGroupedNodes_ForAttributeWithMetadataName: Modified`), o modelo do enum não, e a geração não foi refeita.

**5. Entradas que não geram nada, e as que geram diagnóstico**:

| Entrada | Resultado |
|---|---|
| Enum com `[EnumExtensions]` | 1 arquivo |
| Enum **sem** o atributo | nada |
| Enum no namespace global | 1 arquivo, sem declaração de namespace |
| Enum `internal` | extensão `internal` |
| Enum `private` aninhado | **`AEE001`** (aviso), nenhum arquivo |
| Dois enums de mesmo nome, namespaces diferentes | 2 arquivos, hint names distintos |

**6. Como o teste roda o gerador**:

```csharp
GeneratorDriver driver = CSharpGeneratorDriver.Create(
    generators: [new EnumExtensionsGenerator().AsSourceGenerator()],
    driverOptions: new GeneratorDriverOptions(
        disabledOutputs: IncrementalGeneratorOutputKind.None,
        trackIncrementalGeneratorSteps: true));   // <- sem isto, nao ha como inspecionar o cache
```

Uma armadilha ao testar: conferir erros na compilação **original** não funciona — o atributo injetado não existe lá, e o resultado é um `CS0246` enganoso. O harness expõe a compilação **de saída**, que é onde a verificação faz sentido.

Relação com os vizinhos: `MySimpleSdk` cobre estrutura de biblioteca, `ResilientHttpSdk` o desenho de API de um SDK, e `NuGetPackageLifecycleDemo` a distribuição. Aqui o assunto é código que se escreve durante a compilação.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview
- https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md
- https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md
- https://andrewlock.net/series/creating-a-source-generator/
