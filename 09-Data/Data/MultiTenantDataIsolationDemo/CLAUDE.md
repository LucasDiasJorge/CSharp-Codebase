# CLAUDE.md — MultiTenantDataIsolationDemo

Console com isolamento por tenant via query filter global, índice composto e as quatro formas de furar o isolamento. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 09-Data/Data/MultiTenantDataIsolationDemo/MultiTenantDataIsolationDemo.csproj
dotnet run --project 09-Data/Data/MultiTenantDataIsolationDemo/MultiTenantDataIsolationDemo.csproj
```

Roda oito cenários (0 a 7) e termina. Sem serviço externo — SQLite em arquivo.

## Estrutura interna

`Data/BillingDbContext.OnModelCreating` tem as duas peças:

- `HasQueryFilter(invoice => invoice.TenantId == _tenant.CurrentTenantId)` — lê o tenant **em tempo de consulta**, por isso trocar `TenantContext.CurrentTenantId` muda o que as consultas enxergam (é o que o cenário 6 explora).
- Índice `(TenantId, Customer)`, nessa ordem. O cenário 3 prova a diferença com `EXPLAIN QUERY PLAN`.

`SaveChangesAsync` faz duas coisas: preenche `TenantId` em entidades novas e **lança** se uma entidade modificada pertencer a outro tenant. As duas são proteções separadas; não remover nenhuma.

**Os cenários 4, 5 e 6 são vazamentos deliberados.** Eles existem para mostrar que o filtro global não é fronteira de segurança. Não "consertar" o código desses cenários — o comportamento errado é o material didático, e cada um termina com a orientação de como se proteger.

## Pontos de atenção

- TFM `net10.0`. Pacotes: `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.Extensions.Logging.Console`.
- SQLite em `multitenant-demo.db`, ignorado pelo `.gitignore` local. Não versionar.
- **`ExplainAsync` lê a coluna `detail` por ADO direto.** `SqlQueryRaw<string>` sobre `EXPLAIN QUERY PLAN` devolveria só a primeira coluna (um int) — foi o que aconteceu na primeira versão, que imprimia "3" em vez do plano. Manter a leitura por `DbDataReader`.
- `EXPLAIN QUERY PLAN` e `sqlite_master` são **específicos do SQLite**. Trocar de provider quebra o cenário 3 em tempo de execução.
- O cenário 6 depende de **reutilizar o mesmo `DbContext`** e trocar o tenant nele. É exatamente o antipadrão que o cenário denuncia; não refatorar para dois contextos, ou o vazamento some e a lição também.
- O cenário 4 usa `FromSqlRaw` **com** `IgnoreQueryFilters()` — necessário porque o EF tentaria compor o filtro sobre a consulta crua. O ponto continua válido: SQL manual não herda proteção, e quem escreve precisa filtrar.
- Os números do README (2 e 2 faturas no seed, 5 após a inclusão de Elena; totais 350 e 2100) vêm do seed e do cenário 2. Alterar invalida as tabelas.
- Isolamento por schema ou por banco é citado no README como alternativa mais forte, mas **não** implementado — outra escala de projeto.
- **Fronteira com os vizinhos**: [EfCoreRelationshipsDemo](../EfCoreRelationshipsDemo/CLAUDE.md) cobre modelagem, [EfCoreOptimisticConcurrencyDemo](../EfCoreOptimisticConcurrencyDemo/CLAUDE.md) cobre gravação concorrente. `PolicyBasedAuthorizationDemo` (trilha 04) trata da camada acima — autorização sobre um recurso que o usuário já enxerga.
