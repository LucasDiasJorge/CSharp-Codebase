# CacheAside (Padrão Cache-Aside / Lazy Loading)

## Visão geral

O padrão Cache-Aside delega à aplicação a responsabilidade de:
1. Verificar o cache (hit/miss)
2. Carregar do repositório/dados primários em caso de miss
3. Popular o cache com o resultado
4. Invalidation em operações de escrita (garantindo consistência eventual)

Benefícios: redução de latência em leituras repetidas, menor pressão sobre banco, simplicidade de adoção gradual.

## Conceitos abordados

- Exemplo didático sobre CacheAside (Padrão Cache-Aside / Lazy Loading) no contexto de estratégias de cache e integração com Redis.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Mostrar diferença entre hit vs miss com logging claro.
- Ilustrar desenho de chaves e TTL distintos por tipo de consulta.
- Evidenciar pontos para futura migração a Redis (sem alterar domínio).
- Enfatizar invalidação seletiva para evitar dados obsoletos.

## Estrutura do projeto

```text
CacheAside/
+-- Controllers/
|   +-- CacheController.cs
|   \-- ProductsController.cs
+-- Data/
|   \-- ApplicationDbContext.cs
+-- Interfaces/
|   +-- ICacheService.cs
|   +-- IProductRepository.cs
|   \-- IProductService.cs
+-- Models/
|   \-- Product.cs
+-- Properties/
|   \-- launchSettings.json
+-- Repositories/
|   \-- ProductRepository.cs
+-- Services/
|   +-- MemoryCacheService.cs
|   \-- ProductService.cs
+-- appsettings.Development.json
\-- ...
```

## Como executar

```bash
dotnet run --project 06-Caching/Caching/CacheAside/CacheAside.csproj
```

Swagger: `https://localhost:5001` (ou porta atribuída). Testes básicos em `/api/products`.

## Boas práticas e pontos de atenção

- Chaves previsíveis e hierárquicas.
- Logging de hit/miss (observabilidade de efetividade).
- Invalidação específica (minimiza thundering herd).
- Camadas separadas (Controller -> Service -> Repository -> CacheService).
- Tipos explícitos (ensino).

### 9. Pontos de Atenção

- Primeira requisição de cada chave paga o custo do banco (miss). 
- Invalidação incorreta gera inconsistência ou desperdício de memória.
- Crescimento de variação de filtros = explosão de chaves (avaliar limites / prefixos).

## Conteúdo complementar

##### 3. Estrutura Principal

```
CacheAside/
  Controllers/ (ProductsController, CacheController)
  Data/ (ApplicationDbContext)
  Interfaces/ (ICacheService, IProductRepository, IProductService)
  Models/ (Product)
  Repositories/ (ProductRepository)
  Services/ (MemoryCacheService, ProductService)
  Program.cs
```

##### 4. Fluxo de Leitura (Pseudo)

```csharp
public async Task<Product?> GetProductByIdAsync(int id)
{
    string cacheKey = string.Format("product:{0}", id);
    Product? cached = await _cacheService.GetAsync<Product>(cacheKey);
    if (cached != null) return cached;          // HIT

    Product? product = await _productRepository.GetByIdAsync(id); // MISS -> DB
    if (product != null)
        await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10));
    return product;
}
```

##### 6. Endpoints Principais

| Rota | Método | Descrição |
|------|--------|-----------|
| /api/products | GET | Lista produtos (TTL menor) |
| /api/products/{id} | GET | Produto por Id (TTL maior) |
| /api/products/category/{category} | GET | Produtos por categoria |
| /api/products | POST | Cria e invalida caches relacionados |
| /api/products/{id} | PUT | Atualiza + atualiza chave do item + invalida listas |
| /api/products/{id} | DELETE | Remove e invalida chaves agregadas |
| /api/cache/{key} | DELETE | Remove chave específica |

##### 7. Estratégia de Chaves & TTL

| Tipo | Formato | TTL (min) |
|------|---------|-----------|
| Produto | `product:{id}` | 10 |
| Lista Geral | `products:all` | 5 |
| Por Categoria | `products:category:{slug}` | 7 |

Critérios: listas expiram antes para refletir melhor novas inserções; item individual guarda maior permanência para otimizar leitura repetida.

##### 10. Extensão Futura

- Implementar Redis (`IDistributedCache` ou `StackExchange.Redis`).
- Métricas de hit ratio + Prometheus.
- Cache warming (pre-load de produtos mais acessados).
- Policies de fallback e circuit breaker em falhas de backend.

##### 12. Aprendizados Esperados

Após estudar: entender fluxo Cache-Aside, TTL balanceado, importância de chaves consistentes e invalidação seletiva.

## Referências

- Microsoft Docs – Caching
- Padrão Cache-Aside (Azure Architecture Center)
- Redis Patterns (redis.io)
