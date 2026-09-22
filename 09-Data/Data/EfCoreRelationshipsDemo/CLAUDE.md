# CLAUDE.md — EfCoreRelationshipsDemo

Console com os três tipos de relacionamento do EF Core, owned types, N+1 medido e tracking. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 09-Data/Data/EfCoreRelationshipsDemo/EfCoreRelationshipsDemo.csproj
dotnet run --project 09-Data/Data/EfCoreRelationshipsDemo/EfCoreRelationshipsDemo.csproj
```

Roda sete cenários (0 a 6) e termina. **Sem serviço externo** — SQLite em arquivo, diferente da maioria da trilha, que exige MySQL/Postgres/Mongo.

## Estrutura interna

`Data/QueryCounter` é um `DbCommandInterceptor` que conta execuções de reader. **É o instrumento que torna o N+1 um número** (4 contra 1 consultas); sem ele o cenário 5 seria só uma afirmação.

`Data/BlogDbContext.OnModelCreating` tem os quatro mapeamentos, cada um com um comentário sobre a decisão que representa. `OnConfiguring` registra o interceptor.

O cenário 4 consulta `pragma_table_info` e `sqlite_master` por SQL cru para **provar** que o owned type virou coluna e que não há tabela `Address`. Isso amarra o exemplo ao SQLite; trocar de provider exige reescrever essas duas consultas.

`EnsureDeletedAsync` + `EnsureCreatedAsync` no cenário 0: o banco é recriado a cada execução, então os números do README são reproduzíveis.

## Pontos de atenção

- TFM `net10.0`. A trilha é majoritariamente `net9.0`/`net6.0`, mas a máquina não tem o runtime 9.0. Pacotes: `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.Extensions.Logging.Console`.
- SQLite em `relationships-demo.db`, ignorado pelo `.gitignore` local. **Não versionar** (o vizinho `TransactionalOrderApi` versiona o dele; não copiar).
- O `DbContext` recebe o `QueryCounter` pelo construtor e configura a conexão em `OnConfiguring` — sem DI, porque é console. Ao portar para API, mover para `AddDbContext`.
- Os números do cenário 5 (4 contra 1) dependem de haver **3 posts** no seed. Alterar o seed muda a conta do README.
- As duas consultas SQL cruas do cenário 4 são **específicas do SQLite**. Se o projeto mudar de provider, elas quebram em tempo de execução, não de compilação.
- `AsSplitQuery` e lazy loading são **citados no README mas não implementados**: o primeiro exigiria vários `Include` de coleção para o produto cartesiano aparecer, o segundo exigiria o pacote de proxies. Ambos estão como encaminhamento, não omissão.
- **Fronteira com os vizinhos**: `sqlite-sample-api` e `MoneyStorageApi` usam EF Core em API; `Dapper`/`DapperExample` mostram a alternativa sem ORM. Aqui o assunto é exclusivamente modelagem de relacionamento e carregamento — não expandir para CRUD ou API.
