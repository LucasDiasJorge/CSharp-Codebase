# CLAUDE.md — InvoiceThrottlingApi

API que aplica throttling ao processar um lote de 1000 notas fiscais. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/InvoiceThrottlingApi/InvoiceThrottlingApi.csproj
dotnet run --project 03-WebAPIs/InvoiceThrottlingApi/InvoiceThrottlingApi.csproj
```

## Estrutura interna

- `InvoiceProgram.cs` — ponto de entrada (**não** `Program.cs`).
- `InvoiceGenerator.cs` — gera a massa de 1000 notas.
- `InvoiceProcessor.cs` — onde o controle de taxa acontece, usando `System.Threading.RateLimiting`.
- `InvoiceController.cs`, `InvoiceModels.cs` — endpoint e contratos.

## Pontos de atenção

- **Nomenclatura fora do padrão**: os arquivos usam prefixo `Invoice` em vez da estrutura de pastas do restante da trilha (`Controllers/`, `Models/`, `Services/`), e a configuração é `InvoiceAppSettings.json` em vez de `appsettings.json`. Confirme o carregamento dessa configuração antes de assumir que valores de `appsettings` são lidos.
- TFM `net9.0`. Pacotes: `System.Threading.RateLimiting` 9.0.0, `Swashbuckle.AspNetCore` 7.2.0.
- Distinto de `IdempotencyCacheApi`: aqui limita-se **taxa**, lá evita-se **repetição**.
