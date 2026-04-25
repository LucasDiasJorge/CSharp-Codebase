# Use Cases em C#

## Visão geral

### O que são Use Cases?

**Use Cases** (Casos de Uso) representam as ações ou funcionalidades que um sistema oferece aos seus usuários. Na arquitetura de software, especialmente na **Clean Architecture**, os Use Cases encapsulam a lógica de negócio da aplicação.

### Exemplos neste Projeto

| Exemplo | Descrição |
|---------|-----------|
| [CreateUser](./Examples/CreateUser/) | Criação de usuário com validações |
| [TransferMoney](./Examples/TransferMoney/) | Transferência bancária com regras de negócio |
| [ProcessOrder](./Examples/ProcessOrder/) | Processamento de pedidos de e-commerce |
| [AuthenticateUser](./Examples/AuthenticateUser/) | Autenticação com JWT e proteção brute-force |

## Conceitos abordados

- Exemplo didático sobre Use Cases em C# no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Use Cases em C# se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
UseCases/
+-- Core/
|   +-- IUnitOfWork.cs
|   +-- IUseCase.cs
|   \-- Result.cs
+-- Examples/
|   +-- AuthenticateUser/
|   +-- CreateUser/
|   +-- ProcessOrder/
|   \-- TransferMoney/
+-- Program.cs
\-- UseCases.csproj
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/UseCases/UseCases.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### 1. Single Responsibility (Responsabilidade Única)

Cada Use Case deve fazer **apenas uma coisa** e fazê-la bem.
Cada arquivo deve conter **apenas uma classe/interface**.

##### 2. Independência de Framework

Use Cases não devem depender de frameworks externos, banco de dados ou UI.

##### 3. Testabilidade

Devem ser facilmente testáveis de forma isolada.

##### 4. Inversão de Dependência

Dependem de abstrações (interfaces), não de implementações concretas.

##### Estrutura de Pastas

```
UseCases/
├── Core/
│   ├── IUseCase.cs           # Interface base
│   ├── IUnitOfWork.cs        # Padrão Unit of Work
│   └── Result.cs             # Padrão Result
├── Examples/
│   ├── CreateUser/
│   │   ├── DTOs/
│   │   │   ├── CreateUserInput.cs
│   │   │   └── CreateUserOutput.cs
│   │   ├── Entities/
│   │   │   └── User.cs
│   │   ├── Interfaces/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── IPasswordHasher.cs
│   │   │   └── INotificationService.cs
│   │   ├── CreateUserUseCase.cs
│   │   └── README.md
│   ├── TransferMoney/
│   │   ├── DTOs/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   ├── TransferMoneyUseCase.cs
│   │   └── README.md
│   ├── ProcessOrder/
│   │   ├── DTOs/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   ├── ProcessOrderUseCase.cs
│   │   └── README.md
│   └── AuthenticateUser/
│       ├── DTOs/
│       ├── Entities/
│       ├── Interfaces/
│       ├── AuthenticateUserUseCase.cs
│       └── README.md
├── Program.cs
├── UseCases.csproj
└── README.md
```

##### Padrão de Implementação

```csharp
// Core/IUseCase.cs
public interface IUseCase<TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct = default);
}
```

##### Organização de Arquivos

Cada Use Case segue a estrutura:

| Pasta | Conteúdo |
|-------|----------|
| `DTOs/` | Data Transfer Objects (Input/Output) |
| `Entities/` | Entidades de domínio |
| `Interfaces/` | Ports (abstrações de dependências) |
| `*UseCase.cs` | Implementação do caso de uso |

##### Benefícios desta Organização

✅ **Single Responsibility** - Um arquivo = Uma responsabilidade  
✅ **Facilidade de navegação** - Estrutura previsível  
✅ **Merge conflicts reduzidos** - Arquivos menores e focados  
✅ **Reutilização** - Entidades e interfaces podem ser compartilhadas  
✅ **Testabilidade** - Fácil de mockar dependências

## Referências

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Use Case Driven Development](https://en.wikipedia.org/wiki/Use_case)
