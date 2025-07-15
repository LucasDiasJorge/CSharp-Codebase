# Design Patterns em C#

Este repositório contém implementações de diversos Design Patterns em C# usando projetos de console. Cada projeto demonstra um padrão específico com exemplos práticos e bem documentados.

## 📁 Estrutura do Projeto

```
DesignPattern/
├── Builder/          # Padrão Builder
├── Strategy/         # Padrão Strategy  
├── Factory/          # Padrão Factory
└── DesignPattern.sln # Solution principal
```

## 🏗️ Padrões Implementados

### 1. Builder Pattern
**Localização:** `Builder/`

O padrão Builder é usado para construir objetos complexos passo a passo. Permite criar diferentes representações de um objeto usando o mesmo processo de construção.

#### 📋 Características:
- ✅ Construção fluente de objetos
- ✅ Validação de dados obrigatórios
- ✅ Interface clara e legível
- ✅ Separação entre construção e representação

#### 🔧 Implementação:
```csharp
var pedido = new Pedido.Builder()
    .ComCliente("Lucas Jorge")
    .ComProduto("Notebook")
    .ComQuantidade(1)
    .ComObservacoes("Entregar após às 18h")
    .ComEntregaExpressa()
    .Build();
```

#### 📁 Arquivos:
- `Pedido.cs` - Classe principal com Builder interno
- `Program.cs` - Demonstração de uso

---

### 2. Strategy Pattern
**Localização:** `Strategy/`

O padrão Strategy define uma família de algoritmos, encapsula cada um deles e os torna intercambiáveis. Permite que o algoritmo varie independentemente dos clientes que o utilizam.

#### 📋 Características:
- ✅ Algoritmos intercambiáveis em tempo de execução
- ✅ Elimina condicionais complexas
- ✅ Facilita adição de novos algoritmos
- ✅ Seguimento do princípio Aberto/Fechado

#### 🔧 Implementação:
```csharp
var processador = new ProcessadorPagamento(new CartaoCreditoStrategy());
processador.ProcessarPagamento(150.00m);

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
