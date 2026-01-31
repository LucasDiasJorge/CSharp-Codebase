# Repository Pattern

## Descrição

O **Repository Pattern** fornece uma abstração sobre a camada de persistência, isolando a lógica de acesso a dados do resto da aplicação.

## Estrutura

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

## Diagrama

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

## Benefícios

✅ **Abstração** - Código não conhece detalhes de persistência  
✅ **Testabilidade** - Fácil criar mocks para testes  
✅ **Flexibilidade** - Trocar implementação sem afetar domínio  
✅ **Single Responsibility** - Centraliza acesso a dados  

## Exemplo de Uso

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

## Interface Genérica vs Específica

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
