# CLAUDE.md — PriorityQueueDemo

Console sobre `PriorityQueue<TElement, TPriority>`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 10-Algorithms/PriorityQueueDemo/PriorityQueueDemo.csproj
dotnet run --project 10-Algorithms/PriorityQueueDemo/PriorityQueueDemo.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): enfileiramento com prioridade, remoção por menor prioridade e comparador customizado.

Dois pontos que costumam surpreender e valem preservar na demonstração: a fila é um **min-heap** (menor prioridade sai primeiro — para inverter, use comparador ou inverta o sinal), e **não é estável** (elementos de prioridade igual não saem em ordem de inserção).

## Pontos de atenção

- TFM `net9.0`, sem dependências externas — `PriorityQueue` está no runtime desde .NET 6.
- Não confundir com `05-Messaging/QueueExample` (FIFO puro).
