# Repository Pattern

## Visão geral

Projeto didático do CSharp-101 dedicado a Repository Pattern, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Repository Pattern no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Repository Pattern se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
Repository/
+-- Entities/
|   \-- Product.cs
+-- Implementations/
|   \-- InMemoryProductRepository.cs
\-- Interfaces/
    \-- IProductRepository.cs
```

## Como executar

Consulte o código desta pasta e os projetos relacionados antes de executar comandos específicos.

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

O **Repository Pattern** fornece uma abstração sobre a camada de persistência, isolando a lógica de acesso a dados do resto da aplicação.

##### Estrutura

```
Repository/
├── Entities/
│   └── Product.cs
├── Interfaces/
│   └── IProductRepository.cs
├── Implementations/
│   └── InMemoryProductRepository.cs
└── README.md
```

##### Diagrama

```
┌─────────────────┐     ┌─────────────────────┐     ┌─────────────────┐
│   Application   │────▶│  IProductRepository │◀────│  Data Source    │
│   (Use Cases)   │     │     (Interface)     │     │  (DB, Memory)   │
└─────────────────┘     └─────────────────────┘     └─────────────────┘
                                  △
                                  │
                        ┌─────────────────────┐
                        │ InMemoryRepository  │
                        │ EFCoreRepository    │
                        │ DapperRepository    │
                        └─────────────────────┘
```

##### Benefícios

✅ **Abstração** - Código não conhece detalhes de persistência  
✅ **Testabilidade** - Fácil criar mocks para testes  
✅ **Flexibilidade** - Trocar implementação sem afetar domínio  
✅ **Single Responsibility** - Centraliza acesso a dados

##### Exemplo de Uso

```csharp
// Injeção de dependência
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Product>> GetLowStockAlerts()
    {
        return await _repository.GetLowStockProductsAsync(threshold: 10);
    }
}
```

##### Interface Genérica vs Específica

```csharp
// Interface genérica (Core)
public interface IRepository<TEntity, TId>
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task AddAsync(TEntity entity);
    // ...
}

// Interface específica (estende com métodos de domínio)
public interface IProductRepository : IRepository<Product, Guid>
{
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
}
```
