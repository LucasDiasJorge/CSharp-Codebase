# CLAUDE.md — FluentValidationUserApi

API REST com validação declarativa via FluentValidation. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/FluentValidationUserApi/FluentValidationUserApi.csproj
dotnet run --project 03-WebAPIs/FluentValidationUserApi/FluentValidationUserApi.csproj
```

Requisições de exemplo em `FluentValidationUserApi.http`.

## Estrutura interna

- `Validators/UserValidator.cs` — as regras, **fora** do modelo. Esse é o ponto do exemplo: validação como objeto próprio, testável isoladamente, em vez de data annotations no DTO.
- `Extensions/ServiceCollectionExtensions.cs` — registro do validador e da integração no pipeline MVC.
- `Models/User.cs`, `Models/UserResponse.cs` — entrada e saída separadas.
- `Controllers/UsersController.cs` — recebe já validado; não repete checagem.

## Pontos de atenção

- TFM `net9.0`. Pacotes: `FluentValidation.AspNetCore` 11.3.1, `Swashbuckle.AspNetCore` 6.5.0.
- `FluentValidation.AspNetCore` está descontinuado upstream em favor do registro manual; o exemplo segue a via automática por simplicidade didática.
