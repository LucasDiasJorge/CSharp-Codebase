# SOLID Examples

## Visão geral

Exemplos práticos dos 5 princípios SOLID em C#, com comparações lado a lado entre implementações incorretas e corretas.

## Conceitos abordados

- **SRP**: Single Responsibility Principle
- **OCP**: Open/Closed Principle
- **LSP**: Liskov Substitution Principle
- **ISP**: Interface Segregation Principle
- **DIP**: Dependency Inversion Principle

## Objetivos de aprendizagem

- Entender cada princípio SOLID com exemplos práticos
- Identificar violações comuns e como corrigi-las
- Aplicar os princípios para código mais manutenível
- Reconhecer trade-offs em diferentes abordagens

## Estrutura do projeto

```text
SOLIDExamples/
+-- DIP/
|   \-- DipExample.cs
+-- ISP/
|   \-- IspExample.cs
+-- LSP/
|   \-- LspExample.cs
+-- OCP/
|   \-- OcpExample.cs
+-- SRP/
|   \-- SrpExample.cs
+-- Program.cs
\-- SOLIDExamples.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/SOLIDExamples/SOLIDExamples.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Estrutura do Projeto

```
SOLIDExamples/
├── SRP/
│   └── SrpExample.cs      # Single Responsibility
├── OCP/
│   └── OcpExample.cs      # Open/Closed
├── LSP/
│   └── LspExample.cs      # Liskov Substitution
├── ISP/
│   └── IspExample.cs      # Interface Segregation
├── DIP/
│   └── DipExample.cs      # Dependency Inversion
├── Program.cs
└── README.md
```

##### 1️⃣ SRP - Single Responsibility Principle

> **"Uma classe deve ter apenas uma razão para mudar"**

| ❌ Incorreto | ✅ Correto |
|--------------|------------|
| `BadInvoice` com 3 responsabilidades | Classes separadas: `Invoice`, `InvoiceRepository`, `InvoicePrinter` |

##### 2️⃣ OCP - Open/Closed Principle

> **"Aberto para extensão, fechado para modificação"**

| ❌ Incorreto | ✅ Correto |
|--------------|------------|
| `if/else` para cada tipo de cliente | Interface `ICustomer` extensível |

##### 3️⃣ LSP - Liskov Substitution Principle

> **"Subclasses devem ser substituíveis por suas superclasses"**

| ❌ Incorreto | ✅ Correto |
|--------------|------------|
| `OstrichBad` lança exceção em `Fly()` | Interface `IFlyingBird` separada |

##### 4️⃣ ISP - Interface Segregation Principle

> **"Clientes não devem depender de interfaces que não utilizam"**

| ❌ Incorreto | ✅ Correto |
|--------------|------------|
| `IMachine` com Print, Scan, Fax | Interfaces `IPrinter` e `IScanner` separadas |

##### 5️⃣ DIP - Dependency Inversion Principle

> **"Dependa de abstrações, não de implementações"**

| ❌ Incorreto | ✅ Correto |
|--------------|------------|
| `BadOrderService` depende de `BadLogger` | `OrderService` depende de `ILogger` |

##### Saída do Console

Cada exemplo imprime o contraste entre uma implementação que viola o princípio e uma que segue a boa prática, facilitando o entendimento visual.

## Referências

- [Microsoft - SOLID Principles](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
