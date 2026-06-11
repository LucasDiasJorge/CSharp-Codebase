# Design Patterns em C#

## Visão geral

Projeto didático do CSharp-101 dedicado a Design Patterns em C#, com foco em design patterns, modelagem OO e código limpo.

## Conceitos abordados

- Exemplo didático sobre Design Patterns em C# no contexto de design patterns, modelagem OO e código limpo.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Demonstrar padrões clássicos de design baseados no GoF
- Fornecer exemplos reais e didáticos em C#
- Servir como referência para estudo e consulta
- Aplicar os princípios SOLID junto com os padrões

## Estrutura do projeto

```text
DesignPattern/
+-- Behavioral/
|   +-- ChainOfResponsibility/
|   +-- Mediator/
|   +-- State/
|   +-- Strategy/
|   +-- UnitOfWork/
|   \-- Visitor/
+-- Creational/
|   +-- Builder/
|   \-- Factory/
+-- DesignPattern/
|   +-- Behavioral/
|   +-- Creational/
|   \-- Structural/
+-- Structural/
|   +-- Adapter/
|   +-- Composite/
|   \-- Decorator/
\-- DesignPattern.sln
```

## Como executar

Escolha um dos projetos abaixo para execução direcionada:

- `dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/ChainOfResponsibility/ChainOfResponsability.csproj`
- `dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/Mediator/MediatoR.csproj`
- `dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/State/State.csproj`
- `dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/Strategy/Strategy.csproj`
- `dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/UnitOfWork/UnitOfWork.csproj`
- `dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/Visitor/Visitor.csproj`

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Categorias de Padrões (GoF)

O GoF classificou os 23 padrões em três categorias:

| Categoria | Propósito | Quantidade |
|-----------|-----------|------------|
| **Creational** | Lidam com a criação de objetos | 5 padrões |
| **Structural** | Lidam com a composição de classes e objetos | 7 padrões |
| **Behavioral** | Lidam com a comunicação entre objetos | 11 padrões |

##### Padrões de Criação (Creational Patterns)

> *"Creational patterns abstract the instantiation process."* — GoF

Padrões que abstraem o processo de instanciação, tornando o sistema independente de como os objetos são criados.

| Padrão | GoF Page | Status | Descrição |
|--------|----------|--------|-----------|
| **Abstract Factory** | p. 87 | ⬜ | Fornece interface para criar famílias de objetos relacionados |
| **Builder** | p. 97 | ✅ | Separa a construção de um objeto complexo da sua representação |
| **Factory Method** | p. 107 | ✅ | Define interface para criar objeto, deixando subclasses decidirem |
| **Prototype** | p. 117 | ⬜ | Cria objetos clonando uma instância protótipo |
| **Singleton** | p. 127 | ⬜ | Garante que uma classe tenha apenas uma instância |

##### Builder Pattern (`Creational/Builder/`)

> *"Separate the construction of a complex object from its representation so that the same construction process can create different representations."* — GoF, p. 97

**Problema que resolve:** Criação de objetos complexos com muitos parâmetros opcionais.

```csharp
var relatorio = new RelatorioBuilder()
    .ComTitulo("Vendas 2025")
    .ComAutor("Lucas Jorge")
    .ComSecao("Introdução", "...")
    .Build();
```

##### Factory Method Pattern (`Creational/Factory/`)

> *"Define an interface for creating an object, but let subclasses decide which class to instantiate."* — GoF, p. 107

**Problema que resolve:** Desacoplar o código de criação do código de uso.

```csharp
IVeiculo carro = VeiculoFactory.CriarVeiculo(TipoVeiculo.Carro);
carro.Acelerar();
```

##### Padrões Estruturais (Structural Patterns)

> *"Structural patterns are concerned with how classes and objects are composed to form larger structures."* — GoF

Padrões que lidam com a composição de classes e objetos para formar estruturas maiores.

| Padrão | GoF Page | Status | Descrição |
|--------|----------|--------|-----------|
| **Adapter** | p. 139 | ✅ | Converte interface de uma classe em outra esperada pelo cliente |
| **Bridge** | p. 151 | ⬜ | Desacopla abstração da implementação |
| **Composite** | p. 163 | ✅ | Compõe objetos em estruturas de árvore |
| **Decorator** | p. 175 | ✅ | Adiciona responsabilidades dinamicamente a um objeto |
| **Facade** | p. 185 | ⬜ | Fornece interface unificada para um subsistema |
| **Flyweight** | p. 195 | ⬜ | Usa compartilhamento para suportar muitos objetos granulares |
| **Proxy** | p. 207 | ⬜ | Fornece substituto para controlar acesso a um objeto |

##### Adapter Pattern (`Structural/Adapter/`)

> *"Convert the interface of a class into another interface clients expect. Adapter lets classes work together that couldn't otherwise because of incompatible interfaces."* — GoF, p. 139

**Problema que resolve:** Integrar sistemas legados com código moderno.

```csharp
// Interface moderna
IClientRepository repository = new ClientRepositoryAdapter();
repository.AddClient(new Client { Name = "Lucas", Age = 22 });

// Internamente adapta para sistema legado
```

**Participantes (GoF):**
- **Target** (`IClientRepository`) - Interface que o cliente espera
- **Adaptee** (`LegacyDatabase`) - Interface existente que precisa ser adaptada
- **Adapter** (`ClientRepositoryAdapter`) - Adapta Adaptee para Target

##### Composite Pattern (`Structural/Composite/`)

> *"Compose objects into tree structures to represent part-whole hierarchies. Composite lets clients treat individual objects and compositions of objects uniformly."* — GoF, p. 163

**Problema que resolve:** Tratar objetos individuais e composições uniformemente.

```csharp
Folder root = new Folder("Root");
root.Add(new File("arquivo.txt"));
root.Add(new Folder("SubPasta"));
root.Display(1); // Exibe recursivamente
```

**Participantes (GoF):**
- **Component** (`IComponent`) - Interface comum
- **Leaf** (`File`) - Representa objetos folha
- **Composite** (`Folder`) - Define comportamento para componentes com filhos

##### Decorator Pattern (`Structural/Decorator/`)

> *"Attach additional responsibilities to an object dynamically. Decorators provide a flexible alternative to subclassing for extending functionality."* — GoF, p. 175

**Problema que resolve:** Adicionar funcionalidades sem modificar classes existentes.

```csharp
INotificador notificador = new NotificadorEmail();
notificador = new NotificadorSMS(notificador);
notificador = new NotificadorSlack(notificador);
notificador.Enviar("Mensagem"); // Envia por todos os canais
```

##### Padrões Comportamentais (Behavioral Patterns)

> *"Behavioral patterns are concerned with algorithms and the assignment of responsibilities between objects."* — GoF

Padrões que caracterizam o modo como classes e objetos interagem e distribuem responsabilidades.

| Padrão | GoF Page | Status | Descrição |
|--------|----------|--------|-----------|
| **Chain of Responsibility** | p. 223 | ✅ | Passa requisição por uma cadeia de handlers |
| **Command** | p. 233 | ⬜ | Encapsula requisição como objeto |
| **Interpreter** | p. 243 | ⬜ | Define gramática e interpretador para linguagem |
| **Iterator** | p. 257 | ⬜ | Acessa elementos sequencialmente sem expor representação |
| **Mediator** | p. 273 | ✅ | Define objeto que encapsula interação entre objetos |
| **Memento** | p. 283 | ⬜ | Captura e externaliza estado interno de objeto |
| **Observer** | p. 293 | ⬜ | Define dependência um-para-muitos entre objetos |
| **State** | p. 305 | ✅ | Permite objeto alterar comportamento quando estado muda |
| **Strategy** | p. 315 | ✅ | Define família de algoritmos intercambiáveis |
| **Template Method** | p. 325 | ⬜ | Define esqueleto de algoritmo, delegando passos |
| **Visitor** | p. 331 | ✅ | Representa operação a ser executada em elementos |

##### Chain of Responsibility Pattern (`Behavioral/ChainOfResponsibility/`)

> *"Avoid coupling the sender of a request to its receiver by giving more than one object a chance to handle the request. Chain the receiving objects and pass the request along the chain until an object handles it."* — GoF, p. 223

**Problema que resolve:** Desacoplar remetente de receptor, permitindo múltiplos handlers.

```csharp
var handler = new AutenticacaoHandler();
handler.SetNext(new AutorizacaoHandler())
       .SetNext(new ValidacaoHandler());

handler.Handle(request);
```

**Participantes (GoF):**
- **Handler** - Define interface para tratar requisições
- **ConcreteHandler** - Trata requisições que é responsável
- **Client** - Inicia requisição para um handler da cadeia

##### Mediator Pattern (`Behavioral/Mediator/`)

> *"Define an object that encapsulates how a set of objects interact. Mediator promotes loose coupling by keeping objects from referring to each other explicitly."* — GoF, p. 273

**Problema que resolve:** Centralizar comunicação complexa entre objetos.

```csharp
ChatRoomMediator chatRoom = new ChatRoomMediator();
var alice = new User("Alice");
var bob = new User("Bob");
chatRoom.AddUser(alice);
chatRoom.AddUser(bob);
alice.SendMessage("Olá Bob!"); // Mediador distribui mensagem
```

**Participantes (GoF):**
- **Mediator** (`IMediator`) - Define interface para comunicação
- **ConcreteMediator** (`ChatRoomMediator`) - Implementa comportamento cooperativo
- **Colleague** (`User`) - Conhece apenas seu mediador

##### State Pattern (`Behavioral/State/`)

> *"Allow an object to alter its behavior when its internal state changes. The object will appear to change its class."* — GoF, p. 305

**Problema que resolve:** Eliminar condicionais complexos baseados em estado.

```csharp
var pedido = new Pedido();
pedido.Processar(); // Estado: Pendente → Processando
pedido.Enviar();    // Estado: Processando → Enviado
pedido.Entregar();  // Estado: Enviado → Entregue
```

**Participantes (GoF):**
- **Context** (`Pedido`) - Define interface de interesse e mantém estado atual
- **State** (`IEstadoPedido`) - Define interface para encapsular comportamento
- **ConcreteState** - Implementa comportamento associado ao estado

##### Strategy Pattern (`Behavioral/Strategy/`)

> *"Define a family of algorithms, encapsulate each one, and make them interchangeable. Strategy lets the algorithm vary independently from clients that use it."* — GoF, p. 315

**Problema que resolve:** Eliminar condicionais para seleção de algoritmos.

```csharp
var processador = new ProcessadorPagamento(new CartaoCreditoStrategy());
processador.ProcessarPagamento(100.00m);

// Troca em runtime
processador.SetEstrategia(new PixStrategy());
processador.ProcessarPagamento(75.00m);
```

**Participantes (GoF):**
- **Strategy** (`IPagamentoStrategy`) - Interface comum para algoritmos
- **ConcreteStrategy** - Implementa algoritmo específico
- **Context** (`ProcessadorPagamento`) - Usa Strategy para executar algoritmo

##### Visitor Pattern (`Behavioral/Visitor/`)

> *"Represent an operation to be performed on the elements of an object structure. Visitor lets you define a new operation without changing the classes of the elements on which it operates."* — GoF, p. 331

**Problema que resolve:** Adicionar operações sem modificar classes de elementos.

```csharp
List<IElement> items = new() { new Book("C#", 49.90m), new Dvd("Inception", 29.90m) };

var priceVisitor = new PriceVisitor();
foreach (var item in items)
    item.Accept(priceVisitor);
```

**Participantes (GoF):**
- **Visitor** (`IVisitor`) - Declara operação Visit para cada ConcreteElement
- **ConcreteVisitor** (`PriceVisitor`) - Implementa operações declaradas
- **Element** (`IElement`) - Define operação Accept que recebe visitor
- **ConcreteElement** (`Book`, `Dvd`) - Implementa Accept

##### Unit of Work Pattern (`Behavioral/UnitOfWork/`)

> Nota: Este padrão não é do GoF original, mas é um padrão empresarial catalogado por Martin Fowler em "Patterns of Enterprise Application Architecture" (2002).

**Problema que resolve:** Manter lista de objetos afetados por uma transação de negócio.

```csharp
using var unitOfWork = new UnitOfWork(context);
unitOfWork.Clientes.Add(novoCliente);
unitOfWork.Pedidos.Add(novoPedido);
await unitOfWork.CommitAsync(); // Transação atômica
```

##### Estrutura do Projeto

```
DesignPattern/
├── 📁 Creational/              # Padrões de Criação
│   ├── Builder/                # ✅ Builder Pattern
│   └── Factory/                # ✅ Factory Method Pattern
│
├── 📁 Structural/              # Padrões Estruturais
│   ├── Adapter/                # ✅ Adapter Pattern
│   ├── Composite/              # ✅ Composite Pattern
│   └── Decorator/              # ✅ Decorator Pattern
│
├── 📁 Behavioral/              # Padrões Comportamentais
│   ├── ChainOfResponsibility/  # ✅ Chain of Responsibility Pattern
│   ├── Mediator/               # ✅ Mediator Pattern
│   ├── State/                  # ✅ State Pattern
│   ├── Strategy/               # ✅ Strategy Pattern
│   ├── UnitOfWork/             # ✅ Unit of Work Pattern
│   └── Visitor/                # ✅ Visitor Pattern
│
├── DesignPattern.sln           # Solution File
└── README.md                   # Esta documentação
```

##### Pré-requisitos

- .NET 9.0 ou superior
- Visual Studio Code ou Visual Studio

##### Executando um projeto específico

```powershell
# Exemplo: Executar Strategy Pattern
cd Behavioral/Strategy
dotnet run

# Exemplo: Executar Factory Pattern
cd Creational/Factory
dotnet run

# Exemplo: Executar Adapter Pattern
cd Structural/Adapter
dotnet run
```

##### Compilando toda a solution

```powershell
dotnet build DesignPattern.sln
```

##### Princípios SOLID e Padrões GoF

Os padrões GoF aplicam naturalmente os princípios SOLID:

| Princípio | Sigla | Padrões que Aplicam |
|-----------|-------|---------------------|
| **Single Responsibility** | S | Strategy, State, Visitor |
| **Open/Closed** | O | Decorator, Strategy, Template Method |
| **Liskov Substitution** | L | Factory Method, Strategy |
| **Interface Segregation** | I | Adapter, Facade |
| **Dependency Inversion** | D | Abstract Factory, Builder, Strategy |

##### Citações Essenciais do GoF

> *"Program to an interface, not an implementation."* — GoF, p. 18

> *"Favor object composition over class inheritance."* — GoF, p. 20

> *"Encapsulate the concept that varies."* — GoF

> *"Design patterns should not be applied indiscriminately. Often they achieve flexibility and variability by introducing additional levels of indirection, and that can complicate a design and/or cost you some performance."* — GoF, p. 31

##### Creational

- [ ] **Abstract Factory** - Famílias de produtos relacionados
- [ ] **Prototype** - Clonagem de objetos
- [ ] **Singleton** - Instância única

##### Structural

- [ ] **Bridge** - Separar abstração de implementação
- [ ] **Facade** - Interface simplificada para subsistemas
- [ ] **Flyweight** - Compartilhamento de objetos granulares
- [ ] **Proxy** - Controle de acesso

##### Behavioral

- [ ] **Command** - Encapsular requisições
- [ ] **Interpreter** - Gramáticas e linguagens
- [ ] **Iterator** - Navegação em coleções
- [ ] **Memento** - Snapshot de estado
- [ ] **Observer** - Publicação/Subscrição
- [ ] **Template Method** - Esqueleto de algoritmo

##### Livro Original (GoF)

```
Gamma, E., Helm, R., Johnson, R., & Vlissides, J. (1994).
Design Patterns: Elements of Reusable Object-Oriented Software.
Addison-Wesley Professional.
ISBN: 978-0-201-63361-0
```

##### Livros Complementares

- **Head First Design Patterns** - Freeman & Robson (2004) - Abordagem didática
- **Patterns of Enterprise Application Architecture** - Martin Fowler (2002) - Padrões empresariais
- **Refactoring: Improving the Design of Existing Code** - Martin Fowler (2018) - Refatoração

##### Criacionais (5)

| # | Padrão | Intent (GoF) |
|---|--------|--------------|
| 1 | Abstract Factory | Provide an interface for creating families of related objects |
| 2 | Builder | Separate the construction of a complex object from its representation |
| 3 | Factory Method | Define an interface for creating an object |
| 4 | Prototype | Specify the kinds of objects to create using a prototypical instance |
| 5 | Singleton | Ensure a class only has one instance |

##### Estruturais (7)

| # | Padrão | Intent (GoF) |
|---|--------|--------------|
| 6 | Adapter | Convert the interface of a class into another interface clients expect |
| 7 | Bridge | Decouple an abstraction from its implementation |
| 8 | Composite | Compose objects into tree structures |
| 9 | Decorator | Attach additional responsibilities to an object dynamically |
| 10 | Facade | Provide a unified interface to a set of interfaces |
| 11 | Flyweight | Use sharing to support large numbers of fine-grained objects |
| 12 | Proxy | Provide a surrogate for another object to control access |

##### Comportamentais (11)

| # | Padrão | Intent (GoF) |
|---|--------|--------------|
| 13 | Chain of Responsibility | Give more than one object a chance to handle a request |
| 14 | Command | Encapsulate a request as an object |
| 15 | Interpreter | Define a representation for a language's grammar |
| 16 | Iterator | Provide a way to access elements of an aggregate sequentially |
| 17 | Mediator | Define an object that encapsulates how a set of objects interact |
| 18 | Memento | Capture and externalize an object's internal state |
| 19 | Observer | Define a one-to-many dependency between objects |
| 20 | State | Allow an object to alter its behavior when its internal state changes |
| 21 | Strategy | Define a family of algorithms, encapsulate each one |
| 22 | Template Method | Define the skeleton of an algorithm |
| 23 | Visitor | Represent an operation to be performed on elements |

**Autor:** Lucas Jorge  
**Última Atualização:** Dezembro 2025  
**Tecnologia:** .NET 9.0 / C#

> *"The key to maximizing reuse lies in anticipating new requirements and changes to existing requirements, and in designing your systems so they can evolve accordingly."* — Gang of Four

## Referências

Este repositório é baseado no livro clássico **"Design Patterns: Elements of Reusable Object-Oriented Software"** (1994), escrito por:

- **Erich Gamma**
- **Richard Helm**
- **Ralph Johnson**
- **John Vlissides**

Conhecido como **GoF (Gang of Four)**, este livro catalogou **23 padrões de projeto** divididos em três categorias fundamentais.

### Recursos Online

- [Refactoring Guru - Design Patterns](https://refactoring.guru/design-patterns) - Explicações visuais e exemplos
- [SourceMaking - Design Patterns](https://sourcemaking.com/design_patterns) - Referência completa
- [Microsoft Docs - Design Patterns](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures) - Contexto .NET
- [DoFactory - Design Patterns](https://www.dofactory.com/net/design-patterns) - Implementações em C#
- [NotebookLM](https://notebooklm.google.com/notebook/2eb769f9-4a87-41d3-993e-ebcec9bbdf1f) - Meu NotebookLM de estudos