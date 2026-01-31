# 📚 CQRS Demo - Guia Completo

## 🎯 O que é CQRS?

**CQRS** (Command Query Responsibility Segregation) é um padrão arquitetural que separa as operações de **leitura** (Queries) das operações de **escrita** (Commands) em um sistema.

### Princípio Fundamental

> **"Uma operação deve ser um Command que modifica o estado OU uma Query que retorna dados, mas nunca ambos."**

Este princípio foi introduzido por **Bertrand Meyer** no conceito de **Command-Query Separation (CQS)** e expandido por **Greg Young** para arquitetura de sistemas completos.

---

## 🏗️ Arquitetura do Projeto

```
CQRSDemo/
├── Models/
│   └── Product.cs                  # Entidade de domínio
├── Commands/
│   └── ProductCommands.cs          # Definições de Commands
├── Queries/
│   └── ProductQueries.cs           # Definições de Queries e DTOs
├── Handlers/
│   ├── CommandHandlers.cs          # Processamento de Commands
│   └── QueryHandlers.cs            # Processamento de Queries
├── Infrastructure/
│   └── InMemoryDatabase.cs         # Simulação de banco de dados
└── Program.cs                      # Demonstração prática
```

---

## 📖 Conceitos Principais

### 1️⃣ **Commands** (Escrita)

Commands representam a **intenção de modificar o estado** do sistema.

**Características:**
- ✅ Modificam dados (Create, Update, Delete)
- ✅ Retornam apenas informação de sucesso ou ID
- ✅ Contêm validações de negócio
- ✅ São verbos imperativos (Create, Update, Delete)
- ❌ Nunca retornam dados de domínio

**Exemplo:**
```csharp
public class CreateProductCommand
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

### 2️⃣ **Queries** (Leitura)

Queries representam a **intenção de ler dados** sem modificar o estado.

**Características:**
- ✅ Apenas leem dados
- ✅ Retornam DTOs (Data Transfer Objects)
- ✅ Podem ter filtros, paginação, ordenação
- ✅ São otimizadas para leitura
- ❌ Nunca modificam estado

**Exemplo:**
```csharp
public class GetProductByIdQuery
{
    public Guid Id { get; set; }
}
```

### 3️⃣ **Handlers**

Handlers são responsáveis por **processar** Commands ou Queries.

**Command Handlers:**
- Validam os dados
- Executam lógica de negócio
- Modificam o estado do sistema

**Query Handlers:**
- Buscam dados
- Transformam entidades em DTOs
- Aplicam filtros e ordenação

### 4️⃣ **DTOs** (Data Transfer Objects)

DTOs são objetos simples usados para **transferir dados** das Queries.

**Por quê usar DTOs?**
- 🔒 Não expõe a entidade de domínio diretamente
- 🎯 Retorna apenas os dados necessários
- ⚡ Permite otimizações específicas para leitura
- 🔧 Facilita versionamento de API

---

## 🚀 Como Executar

### 1. Navegar até o diretório do projeto:
```bash
cd CQRSDemo
```

### 2. Restaurar dependências (se necessário):
```bash
dotnet restore
```

### 3. Compilar o projeto:
```bash
dotnet build
```

### 4. Executar o projeto:
```bash
dotnet run
```

---

## 📋 Exemplo de Saída

```
====================================
   CQRS Demo - Sistema de Produtos
====================================

--- PARTE 1: Criando Produtos (COMMANDS) ---

[COMMAND] Produto criado: Notebook Dell (ID: xxx)
[COMMAND] Produto criado: Mouse Logitech (ID: xxx)
[COMMAND] Produto criado: Teclado Mecânico (ID: xxx)

--- PARTE 2: Consultando Produtos (QUERIES) ---

[QUERY] Retornando 3 produtos

Todos os produtos:
  - Notebook Dell: R$ 3.500,00 (Estoque: 15)
  - Mouse Logitech: R$ 350,00 (Estoque: 8)
  - Teclado Mecânico: R$ 450,00 (Estoque: 25)

[QUERY] Produto encontrado: Notebook Dell

Detalhes do produto:
  Nome: Notebook Dell
  Descrição: Notebook Dell Inspiron 15, 16GB RAM, 512GB SSD
  Preço: R$ 3.500,00
  Estoque: 15

--- PARTE 3: Atualizando Produto (COMMAND) ---

[COMMAND] Produto atualizado: Mouse Logitech MX Master 3 (ID: xxx)

--- PARTE 4: Query Especializada ---

[QUERY] Encontrados 1 produtos com estoque baixo (< 10)

Produtos com estoque baixo:
  - Mouse Logitech MX Master 3: 12 unidades

--- PARTE 5: Deletando Produto (COMMAND) ---

[COMMAND] Produto deletado (ID: xxx)

[QUERY] Retornando 2 produtos

Produtos restantes: 2
  - Notebook Dell
  - Mouse Logitech MX Master 3
```

---

## 🎓 Fluxo de Execução

### ➡️ Operação de Escrita (Command)
```
Cliente
  ↓
CreateProductCommand
  ↓
CreateProductCommandHandler
  ↓ (validação)
  ↓ (lógica de negócio)
  ↓
Database (modificação)
  ↓
Retorna ID/Sucesso
```

### ➡️ Operação de Leitura (Query)
```
Cliente
  ↓
GetProductByIdQuery
  ↓
GetProductByIdQueryHandler
  ↓
Database (leitura)
  ↓
Transforma em DTO
  ↓
Retorna ProductDto
```

---

## ✅ Benefícios do CQRS

### 1. **Separação de Responsabilidades**
- Código mais limpo e organizado
- Facilita manutenção e testes
- Cada handler tem uma única responsabilidade

### 2. **Escalabilidade**
- Leitura e escrita podem ser escaladas independentemente
- Permite diferentes bancos de dados para read/write
- Facilita cache e otimizações

### 3. **Performance**
- Queries otimizadas especificamente para leitura
- DTOs podem ter estruturas diferentes do modelo de domínio
- Permite desnormalização de dados para leitura

### 4. **Segurança**
- Commands podem ter validações rigorosas
- Queries nunca modificam estado
- Facilita auditoria e rastreamento

### 5. **Facilita Event Sourcing**
- Commands naturalmente geram eventos
- Histórico completo de mudanças
- Possibilidade de replay de eventos

---

## ⚠️ Quando NÃO usar CQRS?

CQRS adiciona complexidade ao sistema. Não use quando:

- ❌ Aplicação CRUD simples
- ❌ Equipe pequena ou inexperiente com o padrão
- ❌ Domínio de negócio simples
- ❌ Não há necessidade de escalabilidade diferenciada
- ❌ Overhead de código não justifica os benefícios

---

## 🔄 Evolução: CQRS + Event Sourcing

Este projeto demonstra CQRS básico. Para sistemas mais complexos, você pode evoluir para:

```
Command → CommandHandler → Event → EventStore
                              ↓
                          Event Handlers
                              ↓
                        Update Read Models
```

**Bibliotecas populares para CQRS em .NET:**
- **MediatR**: Implementação do padrão Mediator
- **Marten**: Event Store para .NET
- **EventStore**: Database especializado em eventos
- **Brighter**: Framework para Command Dispatcher

---

## 📚 Próximos Passos

### 1. **Adicionar MediatR**
Usar a biblioteca MediatR para simplificar o dispatch de Commands/Queries:

```csharp
// Com MediatR
public class CreateProductCommand : IRequest<Guid>
{
    public string Name { get; set; }
}

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Lógica aqui
    }
}
```

### 2. **Implementar com Entity Framework Core**
Substituir InMemoryDatabase por um banco real:

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
}
```

### 3. **Adicionar Validation**
Usar FluentValidation para validações mais robustas:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### 4. **Implementar API REST**
Criar controllers ASP.NET Core que usam os handlers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly CreateProductCommandHandler _createHandler;
    private readonly GetAllProductsQueryHandler _getAllHandler;

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductCommand command)
    {
        var id = _createHandler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _getAllHandler.Handle(new GetAllProductsQuery());
        return Ok(products);
    }
}
```

### 5. **Adicionar Event Sourcing**
Armazenar eventos ao invés de estado:

```csharp
public class ProductCreatedEvent
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime OccurredAt { get; set; }
}
```

---

## 📖 Referências e Leitura Recomendada

- **Martin Fowler** - [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- **Greg Young** - [CQRS Documents](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
- **Microsoft Docs** - [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- **Livro**: "Domain-Driven Design" - Eric Evans
- **Livro**: "Implementing Domain-Driven Design" - Vaughn Vernon

---

## 🤝 Contribuindo

Este é um projeto didático. Sinta-se livre para:
- Experimentar com o código
- Adicionar novos Commands e Queries
- Implementar validações mais complexas
- Integrar com MediatR
- Adicionar testes unitários

---

## 📝 Licença

Este projeto é para fins educacionais e está disponível gratuitamente.

---

## 💡 Dicas Finais

1. **Comece Simples**: Não implemente CQRS em todos os lugares de uma vez
2. **Use para Bounded Contexts Específicos**: Aplique onde faz sentido
3. **Combine com DDD**: CQRS funciona muito bem com Domain-Driven Design
4. **Pense em Eventos**: Commands devem gerar eventos do domínio
5. **DTOs são Importantes**: Nunca exponha suas entidades diretamente

---

**Feito com 💙 para fins educacionais**
