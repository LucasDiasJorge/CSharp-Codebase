# CLAUDE.md — ProcedureExample

Console que chama **stored procedures** do MySQL a partir de C#. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 mysql
# execute setup.sql no banco antes de rodar

dotnet run --project 09-Data/Data/ProcedureExample/ProcedureExample.csproj
```

## Estrutura interna

- `setup.sql` — **cria as procedures**. Sem executá-lo, o programa falha: a lógica que ele invoca vive no banco, não no C#.
- `Program.cs` — ADO.NET puro com `MySql.Data`: `CommandType.StoredProcedure`, parâmetros de entrada e de saída.

O ponto didático é o parâmetro `OUT` e o mapeamento de resultado — coisas que ORMs escondem.

## Pontos de atenção

- **Exige MySQL ativo e `setup.sql` aplicado.**
- TFM `net9.0`. `MySql.Data` 9.4.0, sem ORM.
- Ao mudar a assinatura de uma procedure em `setup.sql`, ajuste os parâmetros no C# junto — a quebra só aparece em runtime.
