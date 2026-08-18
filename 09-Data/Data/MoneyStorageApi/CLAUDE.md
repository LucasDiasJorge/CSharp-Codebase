# CLAUDE.md — MoneyStorageApi

API que trata **armazenamento correto de valores monetários** no MySQL. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 mysql

dotnet run --project 09-Data/Data/MoneyStorageApi/MoneyStorageApi.csproj
```

Requisições de exemplo em `MoneyStorageApi.http`.

## Estrutura interna

- `src/Domain/Account.cs` e `MoneyMovement.cs` — o saldo é derivado de movimentos, não sobrescrito. Padrão de livro-razão: histórico auditável em vez de um campo mutável.
- `src/Data/MoneyStorageContext.cs` — mapeamento EF Core. **É aqui que está o conteúdo didático**: a precisão da coluna decimal. `decimal` com escala errada, ou pior, `float`/`double`, produz erro de arredondamento em dinheiro.
- `src/Services/AccountService.cs` e `src/DTOs/AccountDtos.cs`.

## Pontos de atenção

- **Exige MySQL ativo**; connection string em `appsettings.json`.
- Nunca troque `decimal` por `double` neste projeto — é exatamente o erro que ele existe para evitar.
- TFM `net9.0`. `MySql.EntityFrameworkCore` 9.0.3.
