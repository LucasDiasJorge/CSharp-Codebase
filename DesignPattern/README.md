
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
| **Adapter**             | Integra interfaces incompatíveis               | `Adapter/`                  |
| **Builder**             | Criação fluente de objetos complexos            | `Builder/`                  |
| **Chain of Responsibility** | Encadeamento de handlers para decisões         | `ChainOfResponsability/`    |
| **Composite**           | Estruturas hierárquicas (folha/composto)       | `Composite/`                |
| **Visitor**             | Separa operações de uma estrutura de objetos   | `Visitor/`                  |
| **Factory**             | Criação flexível de objetos                     | `Factory/`                  |
| **Mediator**            | Centraliza comunicação entre objetos            | `MediatoR/`                 |
| **Strategy**            | Algoritmos intercambiáveis                      | `Strategy/`                 |
| **Unit of Work**        | Transação atômica de múltiplos repositórios     | `UnitOfWork/`               |

## Estrutura

```
DesignPattern/
├── Adapter/               # Adapter Pattern (integração legado)
├── Builder/                # Builder Pattern
├── ChainOfResponsability/  # Chain of Responsibility Pattern
├── Composite/             # Composite Pattern (árvores folha/composto)
├── Visitor/               # Visitor Pattern (separa operações da estrutura)
├── MediatoR/              # Mediator Pattern (sala de chat)
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

### 4. Mediator Pattern
**Localização:** `MediatoR/`

O padrão Mediator reduz o acoplamento direto entre múltiplos objetos ao **centralizar a comunicação** em um componente mediador. No exemplo, uma sala de chat (`ChatRoomMediator`) gerencia usuários (`User`) e repassa mensagens, evitando que cada usuário conheça os demais diretamente.

#### 📋 Características:
- ✅ Comunicação desacoplada (participantes ignoram uns aos outros)
- ✅ Regras de orquestração centralizadas
- ✅ Facilita extensão (ex: filtros, logs, permissões)
- ⚠️ Risco de um "mediador deus" se crescer demais

#### 🔧 Implementação (trecho):
```csharp
ChatRoomMediator chatRoom = new ChatRoomMediator();
var alice = new User("Alice");
var bob = new User("Bob");
chatRoom.AddUser(alice);
chatRoom.AddUser(bob);
alice.SendMessage("Olá Bob!");
```

#### 📁 Arquivos Principais:
- `IMediator.cs` / `IUser.cs` – Abstrações
- `ChatRoomMediator.cs` – Mediador concreto
- `User.cs` – Participante concreto
- `Program.cs` – Demonstração completa

---

### 5. Adapter Pattern
**Localização:** `Adapter/`

O padrão Adapter permite que **interfaces incompatíveis trabalhem juntas** ao atuar como tradutor entre o código moderno e sistemas legados. No exemplo, integra um repositório moderno (`IClientRepository`) com um banco de dados legado (`LegacyDatabase`) que usa dicionários.

#### 📋 Características:
- ✅ Integra sistemas com interfaces incompatíveis
- ✅ Reutiliza código legado sem modificá-lo
- ✅ Isola conversões de dados em local centralizado
- ✅ Facilita testes através de interfaces limpas

#### 🔧 Implementação (trecho):
```csharp
IClientRepository repository = new ClientRepositoryAdapter();
repository.AddClient(new Client { Name = "Lucas", Age = 22 });

// Internamente converte Client → Dictionary<string, object>
// e chama sistema legado: _legacyDb.Insert(record)
```

#### 📁 Organização:
- `Interfaces/IClientRepository.cs` – Interface target moderna
- `Models/Client.cs` – Modelo de domínio
- `Legacy/LegacyDatabase.cs` – Sistema legado (adaptee)
- `Adapters/ClientRepositoryAdapter.cs` – Adapter principal
- `Program.cs` – Demonstração completa

---

### 6. Composite Pattern
**Localização:** `Composite/`

---

### Visitor Pattern
**Localização:** `Visitor/`

O padrão Visitor permite que você adicione operações a objetos de uma estrutura sem modificar as classes desses objetos. Em vez de embutir lógica nas classes de domínio, cria-se um visitante que percorre a estrutura e executa operações específicas.

#### 📋 Características:
- ✅ Separa algoritmos da estrutura de objetos
- ✅ Facilita adição de novas operações sem tocar nas classes de elementos
- ✅ Útil para estruturas heterogêneas (ex: AST, coleções de objetos distintos)
- ⚠️ Requer atualizações nos visitantes quando novas classes de elementos são adicionadas

#### 🔧 Implementação (exemplo incluído):
- `IElement` — interface dos elementos que aceitam visitantes (`Accept(IVisitor)`).
- `IVisitor` — interface do visitante com `Visit` para cada elemento concreto.
- `Book`, `Dvd` — elementos concretos que implementam `IElement`.
- `PriceVisitor`, `ShippingVisitor` — visitantes concretos que realizam operações distintas (cálculo de preço, frete).

Exemplo de uso (demonstrado em `Visitor/Program.cs`):
```csharp
List<IElement> items = new List<IElement>
{
  new Book("C# In Depth", 49.90m),
  new Dvd("Inception", 29.90m, 0.15),
};

PriceVisitor priceVisitor = new PriceVisitor();
ShippingVisitor shippingVisitor = new ShippingVisitor();

foreach (IElement item in items)
{
  item.Accept(priceVisitor);
  item.Accept(shippingVisitor);
}

Console.WriteLine(priceVisitor);
Console.WriteLine(shippingVisitor);
```

#### 📁 Arquivos Principais:
- `IElement.cs` — Interface para elementos
- `IVisitor.cs` — Interface para visitantes
- `Book.cs`, `Dvd.cs` — Elementos concretos
- `PriceVisitor.cs`, `ShippingVisitor.cs` — Visitantes concretos
- `Program.cs` — Demonstração de execução

#### Como executar
```powershell
cd DesignPattern/Visitor
dotnet run
```

---

O padrão Composite permite que objetos individuais (folhas) e composições (pastas) sejam tratados de forma uniforme através de uma interface comum. No exemplo, `File` e `Folder` implementam `IComponent` e a chamada `Display` na raiz exibe recursivamente a árvore.

#### 📋 Características:
- ✅ Tratamento uniforme de folhas e compostos
- ✅ Operações recursivas simplificadas
- ✅ Boa representação para árvores de arquivos, menus e DOMs

#### 🔧 Implementação (trecho):
```csharp
Folder root = new Folder("Root");
root.Add(new File("File A"));
Folder folder1 = new Folder("Folder 1");
folder1.Add(new File("File C"));
root.Add(folder1);
root.Display(1);
```

#### 📁 Arquivos Principais:
- `Program.cs` – Contém `IComponent`, `File`, `Folder` e a demonstração

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 9.0 ou superior
- Visual Studio Code ou Visual Studio

### Executando um projeto específico

1. **Adapter Pattern:**
```bash
cd Adapter
dotnet run
```

2. **Builder Pattern:**
```bash
cd Builder
dotnet run
```

3. **Strategy Pattern:**
```bash
cd Strategy
dotnet run
```

4. **Factory Pattern:**
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
