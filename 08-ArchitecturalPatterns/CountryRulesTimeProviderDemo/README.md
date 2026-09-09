# CountryRulesTimeProviderDemo

## Visão geral

Projeto Web API que demonstra seleção de estratégia por país para retorno de horário local com `TimeProvider`.

A rota recebe a rule desejada, o `StrategyResolver` escolhe a implementação correta e a resposta retorna `TimeProviderNow` já convertido para o timezone do país. O exemplo é simples, mas mostra uma arquitetura que escala bem para projetos com múltiplas regras regionais.

## Conceitos abordados

- Herança com classe base abstrata (`CountryRules`).
- Strategy Resolver com resolução por `AppliesTo` e `Priority`.
- Separação entre regras de domínio (`Rules`) e resolução de estratégia (`Resolvers`).
- `TimeProvider` e `TimeZoneInfo` para multi-timezones.
- Reuso de SDK interno (`ScalarDocumentationSdk`) para documentação OpenAPI.
- Execução containerizada com Docker e configuração de timezone no Alpine.

## Objetivos de aprendizagem

- Evoluir um exemplo OO para uma API reutilizável.
- Resolver regra por chave de entrada sem `switch` em controlador/endpoint.
- Organizar implementação para crescer com novos países sem alterar o endpoint.
- Preparar execução consistente em ambiente local e em container.

## Estrutura do projeto

```text
CountryRulesTimeProviderDemo/
|-- Contracts/
|   |-- CountryTimeErrorResponse.cs
|   `-- CountryTimeResponse.cs
|-- Resolvers/
|   |-- BrazilRuleResolver.cs
|   |-- EnglandRuleResolver.cs
|   |-- ICountryRuleResolver.cs
|   |-- KoreaRuleResolver.cs
|   `-- UnsupportedCountryRuleResolver.cs
|-- Rules/
|   |-- BrazilRules.cs
|   |-- CountryRules.cs
|   |-- EnglandRules.cs
|   `-- KoreaRules.cs
|-- Services/
|   |-- CountryTimeService.cs
|   `-- ICountryTimeService.cs
|-- CountryRulesTimeProviderDemo.csproj
|-- Dockerfile
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/CountryRulesTimeProviderDemo/CountryRulesTimeProviderDemo.csproj
```

Consultar rules disponíveis:

```bash
curl http://localhost:5000/api/time
```

Abrir documentação interativa do Scalar (ambiente Development):

```bash
curl http://localhost:5000/scalar
```

Consultar horário por rule:

```bash
curl http://localhost:5000/api/time/brazil
curl http://localhost:5000/api/time/england
curl http://localhost:5000/api/time/korea
```

Build de validação:

```bash
dotnet build 08-ArchitecturalPatterns/CountryRulesTimeProviderDemo/CountryRulesTimeProviderDemo.csproj
```

Execução via Docker:

```bash
docker build -t country-rules-time-provider-demo 08-ArchitecturalPatterns/CountryRulesTimeProviderDemo
docker run --rm -p 8080:8080 country-rules-time-provider-demo
```

## Boas práticas e pontos de atenção

- O endpoint depende apenas de `ICountryTimeService`, sem conhecer classes concretas de país.
- Para adicionar um novo país, crie uma classe derivada de `CountryRules` e um novo resolver.
- `TimeZoneInfo` pode usar IDs diferentes por sistema operacional; a regra base faz fallback entre Windows e IANA.
- A configuração de documentação fica centralizada no SDK, reduzindo duplicação entre APIs.
- O Dockerfile instala `tzdata` e `icu-libs` para garantir resolução de timezone em Alpine.

## Conteúdo complementar

Regras suportadas atualmente:

- `brazil` (alias `br`)
- `england` (alias `uk`)
- `korea` (alias `kr`)

Exemplo de resposta de sucesso:

```json
{
	"country": "Brazil",
	"rule": "brazil",
	"timeZoneId": "America/Sao_Paulo",
	"initializedAt": "2026-09-08T12:05:34.2148241-03:00",
	"timeProviderNow": "2026-09-08T12:06:02.1816032-03:00"
}
```

## Referências e documentação complementar

- [TimeProvider (Microsoft Learn)](https://learn.microsoft.com/dotnet/api/system.timeprovider)
- [TimeZoneInfo (Microsoft Learn)](https://learn.microsoft.com/dotnet/api/system.timezoneinfo)
- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
