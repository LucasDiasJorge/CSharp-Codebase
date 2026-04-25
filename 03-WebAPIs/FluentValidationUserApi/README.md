# FluentValidation User API

## Visão geral

API REST mínima em ASP.NET Core 9 demonstrando validação declarativa com FluentValidation.

## Conceitos abordados

- **FluentValidation**: Biblioteca para validações tipadas e declarativas
- **Validators Customizados**: Regras de validação reutilizáveis
- **Mensagens de Erro**: Personalização de feedback para o usuário
- **Validação Cruzada**: Validações que dependem de múltiplos campos
- **Integração ASP.NET Core**: Validação automática no pipeline

## Objetivos de aprendizagem

- Implementar validações tipadas com FluentValidation
- Criar mensagens de erro personalizadas e claras
- Aplicar validações cruzadas entre campos relacionados
- Manter código com tipagem explícita para melhor legibilidade

## Estrutura do projeto

```text
FluentValidationUserApi/
+-- Controllers/
|   \-- UsersController.cs
+-- Extensions/
|   \-- ServiceCollectionExtensions.cs
+-- Models/
|   +-- User.cs
|   \-- UserResponse.cs
+-- Properties/
|   \-- launchSettings.json
+-- Validators/
|   \-- UserValidator.cs
+-- appsettings.Development.json
+-- appsettings.json
+-- FluentValidationUserApi.csproj
\-- ...
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/FluentValidationUserApi/FluentValidationUserApi.csproj
```

Acesse o Swagger em: `https://localhost:5001/swagger`

## Boas práticas e pontos de atenção

- Separar validators em arquivos próprios
- Usar `WithMessage()` para mensagens claras
- Aplicar validações no nível de propriedade e de objeto
- Reutilizar validators com `SetValidator()`

## Conteúdo complementar

##### Estrutura do Projeto

```
FluentValidationUserApi/
├── Controllers/
│   └── UsersController.cs    # Endpoints REST
├── Models/
│   ├── User.cs               # Entidade principal
│   └── UserResponse.cs       # DTO de resposta
├── Validators/
│   └── UserValidator.cs      # Regras de validação
├── Program.cs                # Bootstrap da aplicação
└── README.md
```

##### Pré-requisitos

- .NET 9.0 SDK

##### Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/users` | Cria usuário (validação completa) |
| `GET` | `/api/users/{email}` | Busca usuário por email |

##### Exemplo de Payload (POST)

```json
{
  "name": "Ana Silva",
  "email": "ana.silva@example.com",
  "age": 28,
  "dateOfBirth": "1997-03-14"
}
```

##### Exemplo de Validator

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter ao menos 2 caracteres")
            .MaximumLength(100);

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("Formato de e-mail inválido");

        RuleFor(u => u.Age)
            .InclusiveBetween(0, 150).WithMessage("Idade deve estar entre 0 e 150");

        RuleFor(u => u.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Data deve ser no passado");
    }
}
```

##### Personalizando Respostas de Erro

Para customizar o formato de resposta de validação:

```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new { Errors = errors });
        };
    });
```

##### Extensões Futuras

- Adicionar validação assíncrona (ex: verificar email único no banco)
- Implementar `IValidatorInterceptor` para logging
- Criar validators compostos com `Include()`

## Referências

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Integration](https://docs.fluentvalidation.net/en/latest/aspnet.html)
