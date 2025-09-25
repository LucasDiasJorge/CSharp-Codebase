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
