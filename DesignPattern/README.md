
# Design Patterns em C#

## 📚 Visão Geral

Este repositório reúne implementações didáticas dos principais **Design Patterns** (Padrões de Projeto) em C#, cada um em seu próprio subdiretório, com exemplos práticos, código comentado e README explicativo.

## 🎯 Objetivo
- Demonstrar padrões clássicos de design
- Fornecer exemplos reais e didáticos
- Servir como referência para estudo e consulta

## 🏗️ Padrões Implementados

| Padrão                  | Descrição breve                                 | Caminho                     |
|-------------------------|-------------------------------------------------|-----------------------------|
| **Builder**             | Criação fluente de objetos complexos            | `Builder/`                  |
| **Chain of Responsibility** | Encadeamento de handlers para decisões         | `ChainOfResponsability/`    |
| **Factory**             | Criação flexível de objetos                     | `Factory/`                  |
| **Strategy**            | Algoritmos intercambiáveis                      | `Strategy/`                 |
| **Unit of Work**        | Transação atômica de múltiplos repositórios     | `UnitOfWork/`               |

## � Estrutura

```
DesignPattern/
├── Builder/                # Builder Pattern
├── ChainOfResponsability/  # Chain of Responsibility Pattern
├── Factory/                # Factory Pattern
├── Strategy/               # Strategy Pattern
├── UnitOfWork/             # Unit of Work Pattern
└── README.md               # Documentação principal
```

## � Como Executar Exemplos

Cada padrão é um projeto independente. Para rodar um exemplo:

```bash
cd <NomeDoPadrao>
dotnet run
```

Exemplo:
```bash
cd Factory
dotnet run
```

## 🔗 Recursos Adicionais
- [Refactoring Guru - Design Patterns](https://refactoring.guru/design-patterns)
- [Microsoft Docs - Padrões de Projeto](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#design-patterns)

---

💡 **Dica:** Explore cada subdiretório para exemplos, explicações e dicas de uso prático!

// Troca estratégia em runtime
processador.SetEstrategia(new PixStrategy());
processador.ProcessarPagamento(75.00m);
```

#### 📁 Arquivos:
- `Program.cs` - Todas as implementações em um arquivo
  - `IPagamentoStrategy` - Interface da estratégia
  - `CartaoCreditoStrategy` - Estratégia para cartão de crédito
  - `PixStrategy` - Estratégia para Pix
  - `BoletoStrategy` - Estratégia para boleto
  - `ProcessadorPagamento` - Contexto que usa as estratégias

---

### 3. Factory Pattern
**Localização:** `Factory/`

O padrão Factory é usado para criar objetos sem especificar a classe exata do objeto que será criado. Promove o desacoplamento do código de criação do código que usa os objetos.

#### 📋 Características:
- ✅ Criação de objetos sem especificar suas classes concretas
- ✅ Centralização da lógica de criação
- ✅ Facilita manutenção e extensibilidade
- ✅ Suporte a diferentes variações (Simple Factory, Factory Method)

#### 🔧 Implementação:

**Simple Factory:**
```csharp
IVeiculo carro = VeiculoFactory.CriarVeiculo(TipoVeiculo.Carro);
carro.ExibirInfo();
carro.Acelerar();
```

**Factory Method:**
```csharp
var carroEletrico = CarroFactory.CriarCarroEletrico();
carroEletrico.ExibirInfo();
```

#### 📁 Arquivos:
- `IVeiculo.cs` - Interface base para todos os veículos
- `TipoVeiculo.cs` - Enum com tipos de veículos
- `Veiculos.cs` - Implementações concretas (Carro, Moto, Bicicleta, CarroEletrico)
- `VeiculoFactory.cs` - Factories para criação de veículos
- `Program.cs` - Demonstração de uso

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 9.0 ou superior
- Visual Studio Code ou Visual Studio

### Executando um projeto específico

1. **Builder Pattern:**
```bash
cd Builder
dotnet run
```

2. **Strategy Pattern:**
```bash
cd Strategy
dotnet run
```

3. **Factory Pattern:**
```bash
cd Factory
dotnet run
```

### Executando toda a solution
```bash
dotnet build DesignPattern.sln
```

## 📚 Conceitos Aplicados

### Design Principles
- **Single Responsibility Principle (SRP)** - Cada classe tem uma única responsabilidade
- **Open/Closed Principle (OCP)** - Aberto para extensão, fechado para modificação
- **Dependency Inversion Principle (DIP)** - Dependência de abstrações, não de concretizações

### Clean Code
- Nomes descritivos e significativos
- Métodos pequenos e focados
- Separação clara de responsabilidades
- Comentários explicativos quando necessário

### SOLID Principles
Todos os padrões implementados seguem os princípios SOLID:
- **S** - Single Responsibility
- **O** - Open/Closed  
- **L** - Liskov Substitution
- **I** - Interface Segregation
- **D** - Dependency Inversion

## 🎯 Benefícios dos Padrões

| Padrão | Problema que Resolve | Benefício Principal |
|--------|---------------------|---------------------|
| **Builder** | Construção complexa de objetos | Interface fluente e validação |
| **Strategy** | Múltiplos algoritmos condicionais | Flexibilidade e extensibilidade |
| **Factory** | Criação acoplada de objetos | Desacoplamento e centralização |

## 🔄 Próximos Padrões Planejados

- [ ] **Observer** - Notificação de mudanças
- [ ] **Decorator** - Adição dinâmica de funcionalidades
- [ ] **Adapter** - Integração de interfaces incompatíveis
- [ ] **Singleton** - Instância única global
- [ ] **Command** - Encapsulamento de comandos

## 📖 Referências

- [Design Patterns: Elements of Reusable Object-Oriented Software](https://en.wikipedia.org/wiki/Design_Patterns) - Gang of Four
- [Microsoft Docs - Design Patterns](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Refactoring Guru - Design Patterns](https://refactoring.guru/design-patterns)

---

**Autor:** Lucas Jorge  
**Data:** Julho 2025  
**Tecnologia:** .NET 9.0 / C#
