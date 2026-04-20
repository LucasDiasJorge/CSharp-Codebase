# Transfer Money Use Case

## Visão geral

Projeto didático do CSharp-101 dedicado a Transfer Money Use Case, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Transfer Money Use Case no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Transfer Money Use Case se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
TransferMoney/
+-- DTOs/
|   +-- TransferMoneyInput.cs
|   \-- TransferMoneyOutput.cs
+-- Entities/
|   +-- BankAccount.cs
|   +-- Transaction.cs
|   \-- TransactionStatus.cs
+-- Interfaces/
|   +-- IAuditService.cs
|   +-- IBankAccountRepository.cs
|   \-- ITransactionRepository.cs
\-- TransferMoneyUseCase.cs
```

## Como executar

```
┌─────────────────────────────────────────────────────────────────┐
│                   TransferMoneyUseCase                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Validações básicas (valor > 0, contas diferentes)           │
│                          ↓                                       │
│  2. Buscar conta de origem                                      │
│                          ↓                                       │
│  3. Buscar conta de destino                                     │
│                          ↓                                       │
│  4. BEGIN TRANSACTION                                           │
│                          ↓                                       │
│  5. Debitar conta origem (validações de domínio)                │
│      - Conta ativa?                                             │
│      - Saldo suficiente?                                        │
│      - Limite diário OK?                                        │
│                          ↓                                       │
│  6. Creditar conta destino                                      │
│                          ↓                                       │
│  7. Persistir alterações                                        │
│                          ↓                                       │
│  8. Criar registro de transação                                 │
│                          ↓                                       │
│  9. COMMIT TRANSACTION                                          │
│                          ↓                                       │
│  10. Registrar auditoria                                        │
│                          ↓                                       │
│  11. Retornar resultado                                         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

Use Case complexo que demonstra uma transferência bancária entre duas contas, aplicando regras de negócio, transações de banco de dados e auditoria.

##### Regras de Negócio

| Regra | Descrição |
|-------|-----------|
| Valor positivo | Transferência deve ser > 0 |
| Contas diferentes | Origem ≠ Destino |
| Conta ativa | Ambas contas devem estar ativas |
| Saldo suficiente | Origem deve ter saldo >= valor |
| Limite diário | Não exceder limite de transferência |

##### 1. Unit of Work

Garante que todas as operações sejam atômicas - ou tudo funciona, ou tudo é revertido.

##### 2. Rich Domain Model

A entidade `BankAccount` contém lógica de negócio (Withdraw, Deposit) ao invés de ser anêmica.

##### 3. Result Pattern

Evita exceções para fluxos de controle, tornando os erros explícitos.

##### Dependências

| Interface | Responsabilidade |
|-----------|------------------|
| `IBankAccountRepository` | Acesso a dados de contas |
| `ITransactionRepository` | Persistência de transações |
| `IUnitOfWork` | Gerenciamento de transações DB |
| `IAuditService` | Registro de auditoria |

##### Cenários de Erro

```csharp
// Saldo insuficiente
"Saldo insuficiente"

// Conta inativa
"Conta inativa"

// Limite excedido
"Limite diário de transferência excedido. Disponível: R$ 5.000,00"

// Conta não encontrada
"Conta de origem não encontrada"
```

##### Exemplo de Uso

```csharp
var useCase = new TransferMoneyUseCase(
    accountRepository,
    transactionRepository,
    unitOfWork,
    auditService
);

var input = new TransferMoneyInput(
    SourceAccountId: sourceAccountId,
    DestinationAccountId: destinationAccountId,
    Amount: 1000.00m,
    Description: "Pagamento de serviços"
);

var result = await useCase.ExecuteAsync(input);

if (result.IsSuccess)
{
    Console.WriteLine($"Transferência realizada!");
    Console.WriteLine($"ID: {result.Value.TransactionId}");
    Console.WriteLine($"Novo saldo origem: {result.Value.SourceBalance:C}");
}
else
{
    Console.WriteLine($"Falha: {result.Error}");
}
```
