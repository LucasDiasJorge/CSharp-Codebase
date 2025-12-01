# Exemplos de SOLID em C#

Este projeto demonstra exemplos práticos dos princípios SOLID em C#, mostrando lado a lado implementações incorretas e corretas para cada princípio.

## Como executar

1. Abra o terminal na pasta raiz do repositório.
2. Execute:
   ```sh
   dotnet run --project SOLIDExamples/SOLIDExamples.csproj
   ```

## Princípios SOLID demonstrados

### 1. **SRP** — Single Responsibility Principle (Princípio da Responsabilidade Única)
Uma classe deve ter apenas uma razão para mudar, ou seja, deve ter apenas uma responsabilidade.

**Exemplo incorreto:** Classe `BadInvoice` que calcula total, salva no banco e imprime — 3 responsabilidades.

**Exemplo correto:** Classes separadas `Invoice`, `InvoiceRepository` e `InvoicePrinter`, cada uma com sua própria responsabilidade.

### 2. **OCP** — Open/Closed Principle (Princípio Aberto/Fechado)
Entidades de software devem estar abertas para extensão, mas fechadas para modificação.

**Exemplo incorreto:** `BadDiscountCalculator` com if/else que precisa ser modificado para cada novo tipo de cliente.

**Exemplo correto:** Interface `ICustomer` que permite adicionar novos tipos de clientes sem modificar o código existente.

### 3. **LSP** — Liskov Substitution Principle (Princípio da Substituição de Liskov)
Objetos de uma superclasse devem poder ser substituídos por objetos de suas subclasses sem quebrar a aplicação.

**Exemplo incorreto:** `OstrichBad` herda de `Bird` mas lança exceção no método `Fly()`, violando o contrato da classe base.

**Exemplo correto:** Interface `IFlyingBird` separada para pássaros que voam, e classe base `BirdBase` com método abstrato `Move()`.

### 4. **ISP** — Interface Segregation Principle (Princípio da Segregação de Interface)
Clientes não devem ser forçados a depender de interfaces que não utilizam.

**Exemplo incorreto:** Interface `IMachine` com métodos Print, Scan e Fax, forçando `OldPrinter` a implementar métodos que não suporta.

**Exemplo correto:** Interfaces segregadas `IPrinter` e `IScanner`, permitindo implementar apenas o necessário.

### 5. **DIP** — Dependency Inversion Principle (Princípio da Inversão de Dependência)
Módulos de alto nível não devem depender de módulos de baixo nível. Ambos devem depender de abstrações.

**Exemplo incorreto:** `BadOrderService` depende diretamente da implementação concreta `BadLogger`.

**Exemplo correto:** `OrderService` depende da abstração `ILogger`, permitindo trocar a implementação facilmente.

## Estrutura do projeto

```
SOLIDExamples/
├── SRP/
│   └── SrpExample.cs
├── OCP/
│   └── OcpExample.cs
├── LSP/
│   └── LspExample.cs
├── ISP/
│   └── IspExample.cs
├── DIP/
│   └── DipExample.cs
├── Program.cs
└── README.md
```

Cada exemplo imprime no console o contraste entre uma implementação que viola o princípio e uma que segue a boa prática.
