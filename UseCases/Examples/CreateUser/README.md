# Create User Use Case

## Descrição

Este Use Case demonstra a criação de um novo usuário no sistema, aplicando validações, verificações de unicidade e notificações.

## Fluxo de Execução

```
┌─────────────────────────────────────────────────────────────────┐
│                    CreateUserUseCase                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Recebe CreateUserInput (Name, Email, Password, Age)         │
│                          ↓                                       │
│  2. Valida dados de entrada                                      │
│                          ↓                                       │
│  3. Verifica se email já existe no banco                        │
│                          ↓                                       │
│  4. Gera hash da senha                                          │
│                          ↓                                       │
│  5. Cria entidade User                                          │
│                          ↓                                       │
│  6. Persiste no repositório                                     │
│                          ↓                                       │
│  7. Envia email de boas-vindas (async)                          │
│                          ↓                                       │
│  8. Retorna CreateUserOutput                                    │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Regras de Negócio

| Regra | Descrição |
|-------|-----------|
| Nome | Obrigatório, mínimo 2 caracteres |
| Email | Obrigatório, deve conter @ |
| Senha | Obrigatória, mínimo 6 caracteres |
| Idade | Mínimo 18 anos |
| Unicidade | Email deve ser único no sistema |

## Dependências (Ports)

- `IUserRepository` - Acesso a dados de usuário
- `IPasswordHasher` - Serviço de hash de senha
- `INotificationService` - Serviço de notificações

## Exemplo de Uso

```csharp
var useCase = new CreateUserUseCase(
    userRepository,
    passwordHasher,
    notificationService
);

var input = new CreateUserInput(
    Name: "João Silva",
    Email: "joao@email.com",
    Password: "senha123",
    Age: 25
);

var result = await useCase.ExecuteAsync(input);

if (result.IsSuccess)
{
    Console.WriteLine($"Usuário criado: {result.Value.Id}");
}
else
{
    Console.WriteLine($"Erro: {result.Error}");
}
```

## Testes Sugeridos

- ✅ Criação com dados válidos
- ✅ Falha com email duplicado
- ✅ Falha com nome vazio
- ✅ Falha com email inválido
- ✅ Falha com senha curta
- ✅ Falha com idade menor que 18
