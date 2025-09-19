# AtomicOperationsDemo

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
