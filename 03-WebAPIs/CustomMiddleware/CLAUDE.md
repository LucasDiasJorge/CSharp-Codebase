# CLAUDE.md — CustomMiddleware

API que monta um pipeline com quatro middlewares próprios. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/CustomMiddleware/CustomMiddleware.csproj
dotnet run --project 03-WebAPIs/CustomMiddleware/CustomMiddleware.csproj
```

Requisições de exemplo em `CustomMiddleware.http`.

## Estrutura interna

- `Middlewares/` — `CorrelationIdMiddleware`, `CustomHeaderMiddleware`, `RequestResponseLoggingMiddleware`, `RequestTimingMiddleware`.
- `Middlewares/Extensions/UseCustomMiddlewaresExtensions.cs` e `UseDefaultMiddlewaresExtensions.cs` — encapsulam o registro, mantendo `Program.cs` legível.

**A ordem de registro é o conteúdo didático**, não detalhe de implementação: correlation id precisa vir antes do logging para aparecer nos logs, e o timing precisa envolver o resto para medir o pipeline inteiro. Reordenar os `Use*` altera o comportamento observado — não reorganize por estética.

## Pontos de atenção

- TFM `net9.0`. Pacote: `Microsoft.AspNetCore.OpenApi` 9.0.2.
- O middleware de logging lê o corpo da resposta; isso exige buffering. Cuidado ao alterar, é onde esse tipo de código costuma quebrar streaming.
