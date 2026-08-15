# CLAUDE.md — UnitOfWork

Console: múltiplos repositórios coordenados como uma transação lógica única. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Behavioral/UnitOfWork/UnitOfWork.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/UnitOfWork/UnitOfWork.csproj
```

## Estrutura interna

- `CustomerRepository.cs` — acumula as mudanças **sem** persistir.
- `Models.cs` — entidades.
- `Program.cs` — dispara o commit único ao final.

O ponto: repositório não decide quando gravar; quem decide é a unidade de trabalho. Sem isso, cada repositório abre sua própria transação e a atomicidade entre eles se perde.

## Pontos de atenção

- TFM `net9.0`, **sem banco de dados** — tudo em memória, para que o padrão apareça sem ruído de infraestrutura.
- Versões mais realistas do mesmo conceito: `08-ArchitecturalPatterns/PersistencePatterns` e `03-WebAPIs/TransactionalOrderApi` (EF Core + SQLite, com `ITransactionRunner`).
