# BenchmarkTool

## Visão geral

BenchmarkTool é um projeto simples para executar "jobs" computacionalmente pesados e medir seu tempo de execução. Útil para simular carga em máquinas, testar limites de CPU/memória e validar estratégias de timeout/escala.

## Conceitos abordados

- Exemplo didático sobre BenchmarkTool no contexto de testes, benchmarks e validação de comportamento.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como BenchmarkTool se aplica em um cenário prático de testes, benchmarks e validação de comportamento.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
BenchmarkTool/
+-- BenchmarkTool/
+-- Lib/
|   +-- Benchmark.cs
|   +-- IBenchmark.cs
|   \-- Job.cs
+-- .gitignore
+-- BenchmarkTool.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 12-Testing/BenchmarkTool/BenchmarkTool.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Principais Jobs implementados

- `FindPrimes(start, end)` — busca números primos em um intervalo (CPU intensivo). Ex.: `1000000..1010000` (alto custo).
- `MatrixMultiplication(size)` — multiplica duas matrizes `size x size` (CPU + memória). Ex.: `500` ou `800` para cargas pesadas.
- `ComputeHashes(iterations)` — calcula hashes SHA256 com múltiplos rounds (simula carga criptográfica).
- `FibonacciBig(n)` — calcula Fibonacci usando `BigInteger` (memória e CPU ; números grandes geram muitos dígitos).
- `SortLargeList(size)` — gera lista grande e ordena usando *bubble sort* propositalmente ineficiente (O(n²)).
- `StringManipulation(iterations)` — operações intensivas de string e concatenação (uso alto de CPU e GC).

##### Estrutura relevante

- `BenchmarkTool/Program.cs` — runner exemplo que executa vários jobs sequencialmente.
- `BenchmarkTool/Lib/Job.cs` — implementações dos jobs pesados.
- `BenchmarkTool/Lib/Benchmark.cs` — utilitário simples para medir e logar duração (captura exceções).

##### Pré-requisitos

- .NET SDK 6.0+ instalado

##### Build & Run

No diretório raiz do projeto `BenchmarkTool` execute:

```powershell
# Restaurar, compilar e rodar (PowerShell / Windows)
dotnet build
dotnet run --project BenchmarkTool
```

O `Program.cs` atual executa vários jobs em sequência. Para testes rápidos, abra `Program.cs` e ajuste quais jobs serão executados e seus parâmetros.

##### Como ajustar a carga

- Reduza `MatrixMultiplication(500)` para `200` para diminuir uso de RAM/CPU.
- Reduza `FindPrimes` para intervalos menores, por exemplo `10000..11000`.
- Diminuir `ComputeHashes(iterations)` para `100` para testes rápidos.
- Evite `SortLargeList(10000)` em máquinas com pouca RAM — troque por `5000`.

##### Recomendações para tornar mais realista

- Adicionar `CancellationToken` aos jobs para permitir cancelamento seguro.
- Paralelizar tarefas (por exemplo, `Task.Run` com limites usando `Parallel.For/PLINQ`) para simular concorrência.
- Medir uso de CPU/memória via `PerformanceCounter` ou bibliotecas como `Diagnostics.Process`.

##### Segurança e aviso

Estes jobs podem saturar CPU e memória e podem travar a máquina se parâmetros muito altos forem usados. Teste com valores modestos primeiro e monitore o sistema.
