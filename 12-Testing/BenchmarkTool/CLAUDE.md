# CLAUDE.md — BenchmarkTool

Console que executa "jobs" pesados e mede tempo de execução. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 12-Testing/BenchmarkTool/BenchmarkTool.csproj
dotnet run --project 12-Testing/BenchmarkTool/BenchmarkTool.csproj
```

## Estrutura interna

- `Lib/IBenchmark.cs` — contrato.
- `Lib/Job.cs` — a unidade de trabalho pesada.
- `Lib/Benchmark.cs` — executa e cronometra.
- `Program.cs` — dispara a bateria.

Serve para simular carga, testar limites de CPU/memória e validar estratégias de timeout — não para microbenchmark preciso.

## Pontos de atenção

- TFM **`net10.0`**, sem dependências externas.
- **Não é BenchmarkDotNet.** A medição é `Stopwatch` simples, sem warmup, sem múltiplas iterações e sem controle de JIT ou GC — portanto **não** é confiável para comparar implementações. Para isso, o caminho é o pacote BenchmarkDotNet; não cite este projeto como evidência de performance.
- **Consome CPU intensamente** por design.
