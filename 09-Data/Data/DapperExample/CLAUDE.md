# CLAUDE.md — DapperExample

Web API com Dapper sobre MySQL, incluindo transação e hash de senha. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 mysql

dotnet build 09-Data/Data/DapperExample/DapperExample.csproj
dotnet run --project 09-Data/Data/DapperExample/DapperExample.csproj
```

## Estrutura interna

- `src/Database/Scripts/CreateTables.sql` — **execute antes de rodar**; o projeto não cria schema automaticamente (não há EF Core nem migrações).
- `src/Models/BaseModel.cs`, `Company.cs`, `User.cs` — modelos mapeados por convenção do Dapper.
- `Program.cs` — endpoints e as queries SQL escritas à mão.

Diferença central para os projetos EF Core da trilha: aqui o SQL é explícito e o mapeamento é direto, sem change tracker nem migrações.

## Pontos de atenção

- **Exige MySQL ativo** e as tabelas criadas via `CreateTables.sql`.
- TFM `net9.0`. Dapper 2.1.66. **Atenção às outras duas referências**: `mysqlclient` 5.5.2 e `BCrypt` 1.0.0 são pacotes antigos e não-oficiais — os equivalentes atuais são `MySql.Data`/`MySqlConnector` e `BCrypt.Net-Next` (usado no resto do repositório). Não replique este `.csproj` como modelo.
- Layout `src/`: aponte comandos para o `.csproj` na raiz do projeto.
