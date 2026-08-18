# CLAUDE.md — Dapper

Projeto de Web API na trilha de dados. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 09-Data/Data/Dapper/Dapper.csproj
dotnet run --project 09-Data/Data/Dapper/Dapper.csproj
```

Requisições de exemplo em `Dapper.http`.

## Estrutura interna

Apenas `Program.cs` e `WeatherForecast.cs` — é essencialmente o template padrão de Web API do .NET.

## Pontos de atenção

- **O projeto se chama Dapper mas não referencia o Dapper.** A única dependência é `Microsoft.AspNetCore.OpenApi` 9.0.5; não há `PackageReference` para `Dapper`, nenhum acesso a banco e nenhuma query. O conteúdo não corresponde ao nome nem à descrição do índice do README raiz ("Micro ORM Dapper").
- Se a tarefa for estudar Dapper de verdade, use `09-Data/Data/DapperExample`, que tem o pacote, queries, transação e MySQL. Este aqui precisa ser implementado ou removido.
- TFM `net9.0`.
