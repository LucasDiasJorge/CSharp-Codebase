# CLAUDE.md — IncrementalSourceGeneratorDemo

Gerador incremental Roslyn (`ToStringFast()` para enums), consumidor que o carrega como analisador e suíte que testa saída, diagnóstico e cache. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet run -c Release --project 13-SDKsAndLibraries/IncrementalSourceGeneratorDemo/src/Acme.EnumExtensions.Consumer/Acme.EnumExtensions.Consumer.csproj
dotnet test 13-SDKsAndLibraries/IncrementalSourceGeneratorDemo/src/Acme.EnumExtensions.Generator.Tests/Acme.EnumExtensions.Generator.Tests.csproj

# Razoes de cache de cada passo do pipeline
dotnet test .../Acme.EnumExtensions.Generator.Tests.csproj --logger "console;verbosity=detailed"
```

11 testes, todos passando. Layout `src/`, como os vizinhos. Sem serviço externo.

## Armadilhas de TFM e referência (as que mais custam tempo)

- **O gerador é `netstandard2.0` e tem de continuar sendo.** O compilador carrega o analisador no processo dele. Um gerador em `net10.0` **não é carregado**, e o sintoma é "não gera nada" — sem erro, sem aviso. `IsRoslynComponent` e `EnforceExtendedAnalyzerRules` estão ligados.
- `Microsoft.CodeAnalysis.CSharp` com `PrivateAssets="all"`: o consumidor não deve receber Roslyn.
- A referência do consumidor usa `OutputItemType="Analyzer"` + `ReferenceOutputAssembly="false"`. Trocar por `ProjectReference` comum faz o gerador não rodar.
- `EmitCompilerGeneratedFiles` + `CompilerGeneratedFilesOutputPath` no consumidor gravam a saída em `obj/generated/...`. É como se lê o código gerado.

## Estrutura interna

`EnumExtensionsGenerator.Initialize` tem três etapas comentadas: post-initialization do atributo, `ForAttributeWithMetadataName`, `RegisterSourceOutput`.

`Transform` é a **fronteira do cache**: converte `INamedTypeSymbol` em `EnumToGenerate` e nada de Roslyn passa dali. Deixar `ISymbol` ou `SyntaxNode` no modelo derruba o cache e segura memória.

`EnumToGenerate` implementa `IEquatable<T>` à mão, com `SequenceEqual` nos membros. **Não trocar por `record` com `ImmutableArray<string>`**: a igualdade de `ImmutableArray` é por referência e o cache para de funcionar sem que nada quebre visivelmente.

`AnalyzerReleases.Unshipped.md` entra como `AdditionalFiles` e existe para satisfazer `RS2008` — que é exigência legítima, não ruído.

`Acme.EnumExtensions.Generator.Tests/GeneratorHarness` roda o gerador com `trackIncrementalGeneratorSteps: true` e expõe a compilação **de saída**.

## Pontos de atenção

- **Conferir erros na compilação de ENTRADA dá `CS0246` enganoso**: o atributo injetado não existe lá. Usar `GeneratorRun.OutputErrors`, que olha a compilação de saída.
- O atributo tem `[Conditional("ACME_ENUM_EXTENSIONS_KEEP_ATTRIBUTE")]`, então não fica nos metadados do consumidor. Isso é intencional (atributo marcador não precisa sobreviver ao build) e não afeta a geração, que é semântica.
- **A extensão gerada fica no namespace do ENUM.** `Shipping.Carrier.ToStringFast()` exigiu `using Shipping;` no `Program.cs` do consumidor — deu `CS1929` até o using ser acrescentado. Está comentado no código.
- `hintName` inclui o namespace (`Loja.Status.EnumExtensions.g.cs`, `global.Priority...`). Dois enums de mesmo nome em namespaces diferentes colidiriam sem isso; há teste fixando.
- Números medidos no consumidor: `ToString()` 28ms/45,8 MB contra `ToStringFast()` 2ms/0,0 MB em 2 milhões de chamadas. Variam com a máquina; o README cita a ordem de grandeza.
- Razões de cache medidas na segunda execução: entrada idêntica → `SourceOutput: Cached`; classe nova fora do enum → **`Cached`**; membro novo no enum → `Modified`. Esses três casos são os testes de `IncrementalityTests`. Ao mexer no modelo, rodá-los primeiro: é ali que uma regressão de cache aparece.
- `AEE001` é **aviso**, não erro, e o gerador não emite arquivo nesse caso.
- **Fronteira com os vizinhos**: `MySimpleSdk` (estrutura), `ResilientHttpSdk` (desenho de API), `NuGetPackageLifecycleDemo` (distribuição). Não empacotar este gerador como NuGet aqui — o empacotamento de analisador tem convenção própria (`analyzers/dotnet/cs`) e mereceria um exemplo separado.
