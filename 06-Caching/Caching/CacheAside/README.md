<!-- README padronizado (versão condensada) -->
# CacheAside (Padrão Cache-Aside / Lazy Loading)

Projeto demonstrativo do padrão Cache-Aside em API ASP.NET Core (.NET 9, unificado via `Directory.Build.props`). O foco é ensinar fluxo de leitura/escrita com cache em memória, invalidação e boas práticas de chave + expiração. Sem uso de `var` para reforçar clareza didática.

## 1. Visão Geral
O padrão Cache-Aside delega à aplicação a responsabilidade de:
1. Verificar o cache (hit/miss)
2. Carregar do repositório/dados primários em caso de miss
3. Popular o cache com o resultado
4. Invalidation em operações de escrita (garantindo consistência eventual)

Benefícios: redução de latência em leituras repetidas, menor pressão sobre banco, simplicidade de adoção gradual.

## 2. Objetivos Didáticos
- Mostrar diferença entre hit vs miss com logging claro.
- Ilustrar desenho de chaves e TTL distintos por tipo de consulta.
- Evidenciar pontos para futura migração a Redis (sem alterar domínio).
- Enfatizar invalidação seletiva para evitar dados obsoletos.

## 3. Estrutura Principal
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

## 4. Fluxo de Leitura (Pseudo)
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

## 5. Execução Rápida
```powershell
cd "C:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\CacheAside"
dotnet restore
dotnet run
```
Swagger: `https://localhost:5001` (ou porta atribuída). Testes básicos em `/api/products`.

## 6. Endpoints Principais
| Rota | Método | Descrição |
|------|--------|-----------|
| /api/products | GET | Lista produtos (TTL menor) |
| /api/products/{id} | GET | Produto por Id (TTL maior) |
| /api/products/category/{category} | GET | Produtos por categoria |
| /api/products | POST | Cria e invalida caches relacionados |
| /api/products/{id} | PUT | Atualiza + atualiza chave do item + invalida listas |
| /api/products/{id} | DELETE | Remove e invalida chaves agregadas |
| /api/cache/{key} | DELETE | Remove chave específica |

## 7. Estratégia de Chaves & TTL
| Tipo | Formato | TTL (min) |
|------|---------|-----------|
| Produto | `product:{id}` | 10 |
| Lista Geral | `products:all` | 5 |
| Por Categoria | `products:category:{slug}` | 7 |

Critérios: listas expiram antes para refletir melhor novas inserções; item individual guarda maior permanência para otimizar leitura repetida.

## 8. Boas Práticas Aplicadas
- Chaves previsíveis e hierárquicas.
- Logging de hit/miss (observabilidade de efetividade).
- Invalidação específica (minimiza thundering herd).
- Camadas separadas (Controller -> Service -> Repository -> CacheService).
- Tipos explícitos (ensino). 

## 9. Pontos de Atenção
- Primeira requisição de cada chave paga o custo do banco (miss). 
- Invalidação incorreta gera inconsistência ou desperdício de memória.
- Crescimento de variação de filtros = explosão de chaves (avaliar limites / prefixos).

## 10. Extensão Futura
- Implementar Redis (`IDistributedCache` ou `StackExchange.Redis`).
- Métricas de hit ratio + Prometheus.
- Cache warming (pre-load de produtos mais acessados).
- Policies de fallback e circuit breaker em falhas de backend.

## 11. Referências
- Microsoft Docs – Caching
- Padrão Cache-Aside (Azure Architecture Center)
- Redis Patterns (redis.io)

## 12. Aprendizados Esperados
Após estudar: entender fluxo Cache-Aside, TTL balanceado, importância de chaves consistentes e invalidação seletiva.

---
Material versão condensada – conteúdo longo original foi simplificado para padronização geral.
