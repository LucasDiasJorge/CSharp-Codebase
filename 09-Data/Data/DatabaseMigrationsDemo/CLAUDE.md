# CLAUDE.md — DatabaseMigrationsDemo

Console com três migrations reais: criação + seed, coluna aditiva, e expand com backfill. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj
dotnet run --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj
```

Roda cinco cenários e termina. Sem serviço externo — SQLite em arquivo, recriado a cada execução.

Comandos de CLI (exigem `dotnet-ef`; a máquina tem 10.0.10 e avisa que é mais antiga que o runtime 10.0.12 — é só aviso):

```bash
dotnet ef migrations add <Nome> --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj --output-dir Migrations
dotnet ef database update  --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj
dotnet ef migrations script --project 09-Data/Data/DatabaseMigrationsDemo/DatabaseMigrationsDemo.csproj
```

## Estrutura interna

**As migrations em `Migrations/` são reais, geradas por `dotnet ef`** — não escritas à mão. As três formam uma progressão didática e **a ordem importa**:

1. `InitialCreate` — tabela + seed do `HasData`.
2. `AddCustomerPhone` — coluna nula (aditiva).
3. `SplitCustomerNameExpand` — colunas novas **mais um backfill escrito à mão**.

O bloco `migrationBuilder.Sql(...)` no terceiro arquivo foi **acrescentado manualmente** depois do scaffolding: o EF gera schema a partir do diff do modelo, mas mover dados entre colunas é decisão de negócio. Se alguém regenerar essa migration, o backfill se perde.

`ShopDbContext` tem construtor sem parâmetros e `OnConfiguring` com guarda `IsConfigured` — necessário para o `dotnet ef` conseguir instanciar o contexto em tempo de design.

`Demo/MigrationsDemoRunner` usa `IMigrator` (de `Microsoft.EntityFrameworkCore.Infrastructure`, via `GetService`) para migrar a um alvo nomeado, que é como se reverte programaticamente.

## Pontos de atenção

- TFM `net10.0`. Pacotes: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` (necessário para a CLI) e `Microsoft.Extensions.Logging.Console`.
- SQLite em `migrations-demo.db`, ignorado pelo `.gitignore` local. Não versionar.
- **Não editar nem regenerar as migrations existentes.** O backfill manual do terceiro arquivo é parte do material didático e não seria recriado pelo scaffolding.
- Os nomes dos arquivos trazem o timestamp de geração (`20260922...`) e aparecem no README e na saída dos cenários. Regenerar muda os nomes e invalida o texto.
- O cenário 4 reverte para `"AddCustomerPhone"` **pelo nome sem timestamp** — o EF aceita as duas formas. Renomear a migration quebra o cenário.
- O `dotnet ef migrations add` avisa "may result in the loss of data" mesmo quando só o `Down()` é destrutivo. Isso é **esperado** nas migrations 2 e 3, e está explicado no README; não é sinal de problema.
- A etapa **contract** (remover `Name`) **não existe de propósito**: ela quebraria a versão anterior da aplicação, e o ponto do exemplo é que ela vem depois, separada. Não acrescentar.
- A consulta `pragma_table_info` no cenário 4 é específica do SQLite.
- `Database.Migrate()` no startup é usado aqui por conveniência de demo; o README registra que em produção, com várias instâncias, migração costuma ser passo separado do deploy.
- **Fronteira com os vizinhos**: [EfCoreRelationshipsDemo](../EfCoreRelationshipsDemo/CLAUDE.md) cobre modelagem, [EfCoreOptimisticConcurrencyDemo](../EfCoreOptimisticConcurrencyDemo/CLAUDE.md) cobre conflito de gravação. Aqui o assunto é exclusivamente evolução de schema.
