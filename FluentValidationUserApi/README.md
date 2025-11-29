# FluentValidationUserApi

Uma API RESTful minimalista construída com ASP.NET Core 9 e FluentValidation para demonstrar validações tipadas sobre uma única entidade `User`.

## ✨ Objetivos do projeto

- Exibir regras de validação para diferentes tipos de dados (strings, números e datas).
- Utilizar mensagens de erro personalizadas para facilitar a correção do input.
- Manter o código com tipagem explícita, evitando o uso de `var` para reforçar clareza.

# FluentValidationUserApi

API REST mínima em ASP.NET Core 9 que demonstra validação declarativa com FluentValidation aplicada a uma entidade `User`.

Este README foi reorganizado para ser prático: visão geral, como rodar, endpoints, exemplos de validator e dicas para personalizar respostas de erro.

## Visão Geral

- Projeto didático: foco em validators claros, mensagens úteis e separação de responsabilidades.
- Código usa tipagem explícita onde apropriado para melhorar legibilidade (ex.: evitar `var` em declarações complexas).

## Estrutura do Projeto

```
FluentValidationUserApi/
  Controllers/        # Controllers (UsersController)
  Models/             # Entidades/DTOs (User, UserResponse)
  Validators/         # Validators (UserValidator)
  Program.cs          # Bootstrap da aplicação
  FluentValidationUserApi.csproj
```

## Como Rodar (PowerShell)

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\FluentValidationUserApi"
dotnet restore
dotnet run
```

Abra o Swagger em `https://localhost:5001/swagger` (modo Development).

## Endpoints Principais

| Método | Rota | Descrição |
|---|---:|---|
| POST | `/api/users` | Cria um usuário; validação completa do payload |
| GET  | `/api/users/{email}` | Recupera um usuário (demo de CreatedAt/lookup) |

Exemplo de body (POST /api/users):

```json
{
  "name": "Ana Silva",
  "email": "ana.silva@example.com",
  "age": 28,
  "dateOfBirth": "1997-03-14"
}
```

## Exemplo de `UserValidator`

Exemplo reduzido para ilustrar regras e validação cruzada (Age x DateOfBirth):

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

        RuleFor(u => u.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Data de nascimento deve ser no passado");

        RuleFor(u => u.Age)
            .InclusiveBetween(18, 120).WithMessage("Idade deve estar entre 18 e 120 anos")
            .Must((user, age) => age == CalculateAge(user.DateOfBirth))
            .WithMessage("Idade não corresponde à data de nascimento");
    }

    private static int CalculateAge(DateTime dob)
    {
        int years = DateTime.UtcNow.Year - dob.Year;
        if (dob.Date > DateTime.UtcNow.Date.AddYears(-years)) years--;
        return years;
    }
}
```

## Personalizando a resposta de erro (model state)

Por padrão o ASP.NET retorna `ValidationProblemDetails`. Para devolver um formato próprio, configure o `InvalidModelStateResponseFactory` em `Program.cs`:

```csharp
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(err => err.ErrorMessage).ToArray()
            );

        var result = new BadRequestObjectResult(new { Message = "Validation failed", Errors = errors });
        return result;
    };
});
```

Isso retorna um JSON simples com os campos e mensagens associadas.

## Boas Práticas aplicadas

- Valide dados no nível do request — antes de tocar serviços/DB.
- Mantenha `Validator` em classes separadas; controllers apenas delegam.
- Prefira mensagens de erro específicas e localizadas (úteis para APIs públicas).
- Use `AddValidatorsFromAssemblyContaining<T>()` para registrar validators automaticamente.

## Testes recomendados

- Unit tests para `UserValidator` (ex.: FluentValidation.TestHelper).
- Testes de integração para verificar o pipeline (requests inválidos → 400 com payload esperado).

## Extensões futuras

- Validações assíncronas (ex.: verificação de e-mail único no banco).
- Cenários de atualização (validações condicionais com `When`).
- Internationalização das mensagens de validação.

---

Se quiser, eu posso:

- 1) ajustar o `Program.cs` do projeto para incluir o `InvalidModelStateResponseFactory` automaticamente; ou
- 2) adicionar exemplos de testes unitários para o `UserValidator`.

Diga qual opção prefere e eu implemento.

