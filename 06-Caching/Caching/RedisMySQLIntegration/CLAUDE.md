# CLAUDE.md — RedisMySQLIntegration

Console com cache distribuído (Redis) sobre persistência (MySQL). Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name redis -p 6379:6379 redis
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 mysql

dotnet run --project 06-Caching/Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): consulta ao MySQL com `MySql.Data` (ADO.NET puro, sem ORM) e resultado cacheado no Redis com TTL. A comparação de latência entre hit e miss é a saída do programa.

## Pontos de atenção

- **Exige Redis e MySQL ativos**, além de schema/tabela criados — o projeto não traz script de setup nem `docker-compose.yml`. Para um sample equivalente já empacotado, ver `CacheIncrement` (que tem compose próprio).
- TFM `net9.0`. `MySql.Data` 9.3.0, `StackExchange.Redis` 2.8.37.
- Connection strings estão fixas no código.
