# CLAUDE.md — UseCases

Console: casos de uso no estilo Clean Architecture. É o maior projeto do repositório (47 arquivos). Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/UseCases/UseCases.csproj
dotnet run --project 08-ArchitecturalPatterns/UseCases/UseCases.csproj
```

## Estrutura interna

- `Core/` — `IUseCase` (contrato entrada → saída), `IUnitOfWork` e `Result` (**erro como valor de retorno**, não exceção; repare que os use cases devolvem `Result` em vez de lançar).
- `Examples/` — quatro casos de uso, cada um com a mesma estrutura interna de quatro pastas: `DTOs/`, `Entities/`, `Interfaces/` e a classe `*UseCase`:
  - `CreateUser` — hash de senha e notificação.
  - `AuthenticateUser` — o mais completo: refresh token, auditoria de login, verificação de senha.
  - `ProcessOrder` — cliente, produtos, pagamento, tier de desconto.
  - `TransferMoney` — transferência entre contas.

O padrão importante: **cada caso de uso declara as próprias interfaces** (`IUserRepository`, `IPasswordHasher`, `IJwtTokenGenerator`…) em vez de consumir um contrato compartilhado. É a inversão de dependência levada a sério — o caso de uso define o que precisa, a infraestrutura se adapta.

Ao adicionar um caso de uso, replique a estrutura de quatro pastas; não crie pasta `Shared/` de interfaces, isso desfaz a lição.

## Pontos de atenção

- TFM `net8.0`, **sem dependências externas** e sem implementação de infraestrutura — só as abstrações e as regras. É intencional: o foco é a camada de aplicação isolada.
- Contraste com `TransactionScript` (mesma trilha), que resolve problemas parecidos sem domínio nem inversão.
