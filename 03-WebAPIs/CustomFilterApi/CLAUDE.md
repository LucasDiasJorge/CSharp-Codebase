# CLAUDE.md — CustomFilterApi

API que captura e loga propriedades de modelos via **action filter** dirigido por atributos. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/CustomFilterApi/CustomFilterApi.csproj
dotnet run --project 03-WebAPIs/CustomFilterApi/CustomFilterApi.csproj
```

Requisições de exemplo em `CustomFilterApi.http`.

## Estrutura interna

O mecanismo tem três peças que só fazem sentido juntas:

1. `Attributes/LogPropertyAttribute.cs` — marca propriedades a logar.
2. `Attributes/DisableLogPropertyAttribute.cs` — exceção explícita, com precedência sobre a marcação.
3. `Filters/LogPropertyFilter.cs` — no pipeline de action, lê os atributos por reflection sobre o modelo recebido e emite o log. É aqui que a decisão acontece.

Em paralelo, `Services/SelectedServiceAccessor.cs` + `IBusinessService`/`BusinessServiceA`/`BusinessServiceB` demonstram seleção de implementação por requisição.

## Pontos de atenção

- TFM `net9.0`. Pacotes: `Swashbuckle.AspNetCore` 9.0.5, `Microsoft.AspNetCore.OpenApi` 9.0.5.
- O filtro usa reflection a cada requisição — custo real em produção. É aceitável no exemplo, mas não o cite como padrão de performance.
