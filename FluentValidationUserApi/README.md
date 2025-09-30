# FluentValidationUserApi

Uma API RESTful minimalista construída com ASP.NET Core 9 e FluentValidation para demonstrar validações tipadas sobre uma única entidade `User`.

## ✨ Objetivos do projeto

- Exibir regras de validação para diferentes tipos de dados (strings, números e datas).
- Utilizar mensagens de erro personalizadas para facilitar a correção do input.
- Manter o código com tipagem explícita, evitando o uso de `var` para reforçar clareza.

## 🧱 Estrutura principal

```
FluentValidationUserApi/
├── Controllers/
│   └── UsersController.cs      # Endpoints REST para criar e consultar usuários
├── Models/
<!-- README padronizado (versão condensada) -->
# FluentValidationUserApi

API REST mínima (.NET 9) demonstrando validação declarativa com FluentValidation sobre a entidade `User`. Código utiliza tipos explícitos (sem `var`) para fins didáticos e respostas de erro padronizadas.

## 1. Visão Geral
Exibe regras de validação de campos simples (string, int, DateTime) com mensagens claras. Estrutura separa modelo, DTO de resposta, validator e controller. Retornos inválidos produzem `ValidationProblemDetails` customizado.

## 2. Objetivos Didáticos
- Mostrar configuração enxuta do FluentValidation.
- Demonstrar mensagens personalizadas e coerência `Age` x `DateOfBirth`.
- Ensinar registro automático de validators via assembly scanning.
- Reforçar clareza com tipos explícitos.

## 3. Estrutura
```
FluentValidationUserApi/
  Controllers/ (UsersController)
  Models/ (User, UserResponse)
  Validators/ (UserValidator)
  Program.cs
```

## 4. Configuração Essencial
`FluentValidation.AspNetCore` já referenciada. Em `Program.cs`:
```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
```
Para personalizar resposta de erro: configurar `InvalidModelStateResponseFactory` nos `ApiBehaviorOptions` (retornando mensagens agregadas).

Validator típico (exemplo abreviado):
```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Name)
            .NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(u => u.Email)
            .NotEmpty().EmailAddress();
        RuleFor(u => u.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Date of Birth must be in the past.");
        RuleFor(u => u.Age)
            .InclusiveBetween(18, 120)
            .Must((user, age) => age == CalcularIdade(user.DateOfBirth))
            .WithMessage("Age must match the calculated value based on Date of Birth.");
    }
    private static int CalcularIdade(DateTime dob)
    {
        int years = DateTime.UtcNow.Year - dob.Year;
        if (dob.Date > DateTime.UtcNow.Date.AddYears(-years)) years--;
        return years;
    }
}
```

## 5. Execução
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\FluentValidationUserApi"
dotnet restore
dotnet run
```
Swagger: `https://localhost:5001`.

## 6. Endpoints
| Método | Rota | Descrição |
|--------|------|-----------|
| POST | /api/users | Cria usuário (validação completa) |
| GET | /api/users/{email} | Retorna usuário de exemplo (demo para CreatedAt) |

Exemplo POST:
```json
{ "name": "Ana Silva", "email": "ana.silva@example.com", "age": 28, "dateOfBirth": "1997-03-14" }
```

## 7. Regras (Resumo)
| Campo | Regras |
|-------|--------|
| Name | Obrigatório; 2–100 chars |
| Email | Obrigatório; formato válido |
| Age | 18–120; coerente com DateOfBirth |
| DateOfBirth | Passado; <= 120 anos |

## 8. Boas Práticas Aplicadas
- Tipos explícitos para didática.
- Mensagens de erro específicas (evitam ambiguidades).
- Validação coesa entre campos relacionados (consistência lógica).
- Separação clara: Controller fino, Validator focado em regras, Model simples.

## 9. Extensões Futuras
- Validações assíncronas (e-mail único em repositório).
- Regras condicionais (`When`, `DependentRules`).
- Pipelines de validação por contexto (ex.: criação vs atualização).
- Testes unitários dos validators (ex.: FluentValidation.TestHelper).

## 10. Aprendizados Esperados
Como estruturar validators declarativos, produzir mensagens úteis e manter consistência de dados sem acoplar lógica de negócio à camada de API.

---
Versão original detalhada substituída por formato padronizado condensado.

