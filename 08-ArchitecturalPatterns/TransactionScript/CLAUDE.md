# CLAUDE.md — TransactionScript

Console: lógica de negócio organizada em procedimentos, um por caso de uso. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/TransactionScript/TransactionScript.csproj
dotnet run --project 08-ArchitecturalPatterns/TransactionScript/TransactionScript.csproj
```

## Estrutura interna

- `Core/` — `ITransactionScript`, `ScriptResult`, `IDataGateway` + `InMemoryDataGateway` (acesso a dados simples, sem repositório por agregado).
- `Examples/{CreateInvoice,ProcessRefund,TransferMoney}/` — cada um com seu `Scripts/` e seus `DTOs/` de entrada e saída.

Cada script é um procedimento do começo ao fim: valida, calcula, persiste. **Não há modelo de domínio com comportamento** — e isso é a proposta, não um defeito. É o padrão adequado quando a lógica é simples e não compensa modelar um domínio rico.

## Pontos de atenção

- TFM `net8.0`, tudo em memória.
- Contraponto direto de `08-ArchitecturalPatterns/UseCases` (Clean Architecture) e de `07-DesignPatterns/RichVsAnemicDomain/RichDomain`. A comparação entre os três é o que dá valor a este; não "melhore" este projeto na direção de domínio rico.
