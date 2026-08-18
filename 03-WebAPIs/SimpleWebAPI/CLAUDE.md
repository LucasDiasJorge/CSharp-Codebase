# CLAUDE.md — SimpleWebAPI

API REST básica com controllers e logging estruturado Serilog. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/SimpleWebAPI/SimpleWebAPI.csproj
dotnet run --project 03-WebAPIs/SimpleWebAPI/SimpleWebAPI.csproj
```

## Estrutura interna

- `Controllers/ProductController.cs` — CRUD em memória, no estilo controller clássico (contraste com `MinimalApiDemo`).
- `Models/ProductModel.cs` + `Annotations/PriceAttibute.cs` — atributo de validação customizado.
- `Program.cs` — configura Serilog com sinks de console e arquivo, além do Swagger.

## Pontos de atenção

- TFM `net9.0`. Serilog 4.2.0 (+ `AspNetCore`, `Formatting.Compact`, sinks Console e File), `Swashbuckle.AspNetCore` 8.1.1.
- **O sink de arquivo escreve em disco na execução** — gera artefatos de log no diretório de saída. Não os comite.
- O arquivo do atributo tem typo: `PriceAttibute.cs` (falta o `r`). Renomear afeta o nome do tipo; ajuste os usos junto.
