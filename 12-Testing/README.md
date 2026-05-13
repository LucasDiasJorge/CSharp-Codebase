# 12-Testing

Repositório de exemplos e exercícios sobre testes e benchmarking em C#.

Esta pasta contém projetos de exemplo para aprender e praticar testes unitários, regras de negócio e benchmarking em .NET.

Projetos incluídos

- `BenchmarkTool` — pequena ferramenta para executar benchmarks locais e comparar jobs.
- `OrderRuleConsole` — aplicativo de console que demonstra regras de negócio aplicadas a pedidos.
- `OrderRuleConsole.Tests` — projeto de testes associados ao `OrderRuleConsole`.

Como usar

Pré-requisitos

- .NET SDK compatível com os frameworks alvo nos projetos (ver arquivos `.csproj`).

Executar a aplicação de console

1. Abra um terminal na pasta `12-Testing/OrderRuleConsole`.
2. Rode:

```
dotnet run --project OrderRuleConsole.csproj
```

Executar os benchmarks

1. Abra um terminal na pasta `12-Testing/BenchmarkTool`.
2. Rode:

```
dotnet run --project BenchmarkTool.csproj
```

Executar os testes

1. Abra um terminal na raiz de `12-Testing` ou diretamente em `OrderRuleConsole.Tests`.
2. Rode:

```
dotnet test OrderRuleConsole.Tests/OrderRuleConsole.Tests.csproj
```

Estrutura da pasta

- `BenchmarkTool/` — código e libs para benchmarks (`Lib/Benchmark.cs`, `Lib/Job.cs`, `Lib/IBenchmark.cs`).
- `OrderRuleConsole/` — app de exemplo com `Program.cs` e modelos em `Models/`.
- `OrderRuleConsole.Tests/` — projeto de testes com casos de validação e cobertura para a lógica de regras.

Contribuições

- Sinta-se à vontade para abrir PRs com melhorias, novos testes e benchmarks.
- Mantenha a compatibilidade com as versões do .NET usadas no repositório.

Recursos e próximos passos sugeridos

- Adicionar script `dotnet tool` ou `global.json` para travar a versão do SDK, se desejado.
- Expandir `OrderRuleConsole.Tests` com casos de borda e testes de integração.
- Integrar benchmarks com `BenchmarkDotNet` para resultados mais confiáveis.

Licença

Consulte a licença do repositório principal ou adicione uma licença específica se necessário.