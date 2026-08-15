# CLAUDE.md — TransactionPattern

Console: `ExecuteInTransactionAsync` encapsulando o controle transacional. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/TransactionPattern/TransactionPattern.csproj
dotnet run --project 08-ArchitecturalPatterns/TransactionPattern/TransactionPattern.csproj
```

## Estrutura interna

- `Core/IRepository.cs` + `Core/BaseRepository.cs` — `BaseRepository` expõe `ExecuteInTransactionAsync`, que abre a transação, executa o delegate recebido, faz commit e garante rollback em exceção. O `try/catch/rollback` fica em **um lugar só**.
- `Examples/BankTransferService.cs` — débito e crédito atômicos, o caso onde falha parcial é inaceitável.
- `Examples/OrderService.cs` — segundo cenário sobre a mesma base.

## Pontos de atenção

- TFM `net8.0`. Usa **`System.Data.SqlClient` 4.8.6**, pacote legado e descontinuado — o substituto atual é `Microsoft.Data.SqlClient`. Preservado por ser material didático; não replique em código novo.
- Espera **SQL Server** disponível para execução real; a leitura do padrão dispensa o banco.
- Versão em API web do mesmo conceito, com abstração `ITransactionRunner`: `03-WebAPIs/TransactionalOrderApi`.
