# NullableReferenceTypesDemo

Projeto console que explora nullable reference types, analise de fluxo do compilador, operadores de null e guard clauses para prevenir `NullReferenceException`.

## Visao geral

O exemplo modela perfis de cliente com campos obrigatorios e opcionais. Alguns dados sao anotados como nullable com `?`, enquanto servicos aplicam guard clauses, pattern matching e operadores de null para lidar com ausencia de valor de forma explicita.

Ao ler o codigo, observe como o compilador passa a tratar uma referencia como nao nula depois de checks como `profile is null`, `string.IsNullOrWhiteSpace` e guards anotadas com `[NotNull]`.

## Conceitos abordados

- Nullable reference types com `string?` e objetos opcionais.
- Analise de fluxo do compilador apos guard clauses e pattern matching.
- Operadores `?.`, `??` e `??=`.
- Guard clauses com atributos de nullable analysis.
- Separacao entre ausencia esperada e erro de contrato.

## Objetivos de aprendizagem

- Entender quando uma referencia deve ser anotada como nullable.
- Usar operadores de null para expressar fallback sem esconder regras de negocio.
- Criar guards que ajudam tanto em runtime quanto na analise estatica do compilador.
- Reduzir risco de `NullReferenceException` tornando contratos mais explicitos.

## Estrutura do projeto

```text
NullableReferenceTypesDemo/
|-- Demo/
|   `-- NullableDemoRunner.cs
|-- Models/
|   |-- ContactPreference.cs
|   |-- CustomerProfile.cs
|   |-- ShippingAddress.cs
|   `-- ValidationResult.cs
|-- Services/
|   |-- ContactPreferenceResolver.cs
|   |-- ProfileFormatter.cs
|   `-- RegistrationValidator.cs
|-- Utilities/
|   `-- Guard.cs
|-- NullableReferenceTypesDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 01-Fundamentals/NullableReferenceTypesDemo/NullableReferenceTypesDemo.csproj
```

Para validar compilacao:

```bash
dotnet build 01-Fundamentals/NullableReferenceTypesDemo/NullableReferenceTypesDemo.csproj
```

## Boas praticas e pontos de atencao

- Use `T?` quando `null` for um estado valido e esperado.
- Prefira guard clauses cedo em metodos que exigem valor obrigatorio.
- Evite o operador null-forgiving `!` como solucao padrao; ele silencia o compilador sem validar dados.
- Use `?.` e `??` para fallback simples, mas mantenha regras complexas em metodos nomeados.

## Conteudo complementar

Resumo dos operadores usados:

```text
profile?.Email   -> acessa Email apenas se profile nao for null
value ?? fallback -> usa fallback quando value e null
channels ??= ... -> inicializa channels apenas quando esta null
```

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/csharp/nullable-references
- https://learn.microsoft.com/dotnet/csharp/language-reference/operators/member-access-operators
- https://learn.microsoft.com/dotnet/csharp/language-reference/operators/null-coalescing-operator
