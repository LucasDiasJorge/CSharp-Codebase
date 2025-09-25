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
│   ├── User.cs                 # Entidade validada pelo FluentValidation
│   └── UserResponse.cs         # Payload retornado ao cliente
├── Validators/
│   └── UserValidator.cs        # Regras de validação para o modelo User
├── Program.cs                  # Configuração do pipeline, Swagger e FluentValidation
├── appsettings*.json
└── README.md
```

## ⚙️ Configuração do FluentValidation

1. **Adicionar dependência NuGet** no arquivo `FluentValidationUserApi.csproj`:
   ```xml
   <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
   ```
2. **Registrar os serviços** em `Program.cs`:
   ```csharp
   builder.Services.AddFluentValidationAutoValidation();
   builder.Services.AddFluentValidationClientsideAdapters();
   builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
   ```
3. **Configurar retorno padronizado** para falhas de validação utilizando `ApiBehaviorOptions.InvalidModelStateResponseFactory`.
4. **Implementar um validator** derivando de `AbstractValidator<User>` e descrevendo mensagens personalizadas em cada regra.

### Registro do `AbstractValidator<User>` (UserValidator)

O `UserValidator` é uma classe que herda de `AbstractValidator<User>` e descreve todas as regras de validação para a entidade `User`.

No `Program.cs` nós registramos automaticamente todos os validators presentes na assembly com:

```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
```

O que cada linha faz:

- `AddFluentValidationAutoValidation()` — habilita a validação automática dos modelos pelo pipeline do ASP.NET Core. Quando um `Controller` recebe um `User` no corpo da requisição, o FluentValidation será invocado automaticamente antes do método do controller ser executado.
- `AddFluentValidationClientsideAdapters()` — registra adaptadores que ajudam integrações com validação do lado do cliente (útil em aplicações que usam scaffolding de formulários). Não é obrigatório para APIs REST, mas é uma prática comum adicioná-lo para compatibilidade.
- `AddValidatorsFromAssemblyContaining<UserValidator>()` — procura na assembly atual por classes que implementam `IValidator<T>` (incluindo `AbstractValidator<T>`) e as registra no container de DI com escopo transitório. Isso permite que o ASP.NET Core resolva e execute o `UserValidator` quando um `User` precisar ser validado.

Registro manual (alternativa):

Se preferir registrar o validator explicitamente em vez de escanear a assembly, você pode fazer:

```csharp
builder.Services.AddTransient<IValidator<User>, UserValidator>();
```

Observações sobre comportamento e ciclo de vida:

- Os validators registrados via `AddValidatorsFromAssemblyContaining` são registrados como serviços transitórios. Isso segue a recomendação do FluentValidation para validators, pois eles não devem manter estado entre requisições.
- A validação automática transforma falhas em erros de modelo (`ModelState`) e, com a configuração de `InvalidModelStateResponseFactory`, você pode personalizar a forma como a API retorna os erros (por exemplo, retornar `ValidationProblemDetails` com mensagens claras).
- Validações que dependem de serviços (por exemplo, verificar se um e-mail já existe no banco) podem receber serviços via injeção de dependência no construtor do validator. Nesse caso, assegure-se de registrar os serviços dependentes no container antes de registrar os validators.

Exemplo de validator que injeta um serviço:

```csharp
public class UserValidator : AbstractValidator<User>
{
  public UserValidator(IUserRepository userRepository)
  {
    RuleFor(u => u.Email)
      .NotEmpty()
      .EmailAddress()
      .MustAsync(async (email, cancellation) => !await userRepository.ExistsByEmailAsync(email))
      .WithMessage("Email already registered.");
  }
}
```

Se usar validações assíncronas (`MustAsync`) ou validar com serviços externos, lembre-se de que a performance pode ser afetada por chamadas IO; prefira caches ou validações em lote quando possível.


## 🚀 Como executar

Pré-requisitos:
- .NET SDK 9.0.100 ou superior instalado e configurado no `PATH`.

Passos:

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\FluentValidationUserApi"
dotnet restore
dotnet run
```

A API ficará disponível em `https://localhost:5001` (HTTPS) e `http://localhost:5000` (HTTP).

## 📚 Endpoints

| Método | Rota               | Descrição                                         |
|--------|--------------------|---------------------------------------------------|
| POST   | `/api/users`       | Valida e cria um usuário retornando `201 Created` |
| GET    | `/api/users/{email}` | Retorna um usuário de exemplo para demonstração  |

### Exemplo de requisição `POST /api/users`

```http
POST https://localhost:5001/api/users
Content-Type: application/json

{
  "name": "Ana Silva",
  "email": "ana.silva@example.com",
  "age": 28,
  "dateOfBirth": "1997-03-14"
}
```

Resposta `201 Created`:

```json
{
  "name": "Ana Silva",
  "email": "ana.silva@example.com",
  "age": 28,
  "dateOfBirth": "1997-03-14T00:00:00"
}
```

### Exemplo de retorno com erro de validação

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Review the errors for additional details on how to fix them.",
  "errors": {
    "Email": [
      "Email must be a valid email address (e.g., user@example.com)."
    ],
    "Age": [
      "Age must match the calculated value based on Date of Birth."
    ],
    "DateOfBirth": [
      "Date of Birth must be in the past."
    ]
  }
}
```

## ✅ Regras implementadas

| Propriedade    | Regra                                                                 |
|----------------|----------------------------------------------------------------------|
| `Name`         | Obrigatório, mínimo 2 e máximo 100 caracteres                        |
| `Email`        | Obrigatório, formato de e-mail válido                                |
| `Age`          | Intervalo permitido de 18 a 120 anos e coerente com `DateOfBirth`    |
| `DateOfBirth`  | Obrigatório, data no passado, limite máximo de 120 anos              |

Cada regra possui mensagem personalizada informando exatamente como corrigir o input.

## 🧪 Testando com o arquivo `.http`

Um arquivo `FluentValidationUserApi.http` está disponível na raiz do projeto contendo requisições pré-configuradas para ser usado diretamente no Visual Studio, JetBrains Rider ou VS Code com a extensão REST Client.

## 📝 Observações

- O projeto utiliza controllers ao invés de endpoints minimalistas para reforçar o fluxo tradicional de APIs REST.
- A ausência de `var` é intencional para manter a tipagem explícita em todas as variáveis, conforme requisito.
- O endpoint `GET /api/users/{email}` não persiste dados; ele serve apenas como alvo do `CreatedAtAction` e para facilitar testes rápidos.

Aproveite a base como ponto de partida para explorar cenários mais avançados com FluentValidation, como validações assíncronas, reutilização de regras e testes automatizados!
