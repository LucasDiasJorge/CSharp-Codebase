# AtomicOperationsDemo

## Visão geral

Demonstação de operações atomicas em C# e como aplicá-las em sistemas backend.

Conteúdo:
- Interlocked: operações atômicas em inteiros
- lock (monitor): seções críticas com monitor
- ConcurrentDictionary: atualizações atômicas por chave
- Transações no EF Core: garantir atomicidade em múltiplas operações de banco
- Concorrência otimista (RowVersion) no EF Core
- Lock distribuído com Redis (SET NX + expiry)

Execução rápida:
```powershell
cd "AtomicOperationsDemo/src/AtomicOperationsDemo"
dotnet run
```

Observações:
- Para o demo Redis, tenha um Redis local em `localhost:6379`.
- O demo EF Core usa um arquivo Sqlite `atomic_demo.db` na pasta do projeto.

## Conceitos abordados

- Exemplo didático sobre AtomicOperationsDemo no contexto de assincronia, tasks, threads e coordenação de trabalho.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como AtomicOperationsDemo se aplica em um cenário prático de assincronia, tasks, threads e coordenação de trabalho.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
AtomicOperationsDemo/
\-- src/
    \-- AtomicOperationsDemo/
```

## Como executar

Escolha um dos projetos abaixo para execução direcionada:

- `dotnet run --project 02-AsyncAndConcurrency/AtomicOperationsDemo/src/AtomicOperationsDemo/AtomicOperationsDemo.csproj`

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.
