# Cache Patterns in C# 🚀

Este projeto demonstra a implementação de 8 padrões arquiteturais de cache em C#, mostrando diferentes estratégias para otimização de performance e gerenciamento de dados.

## 📋 Padrões Implementados

### 1. **Cache-Aside (Lazy-Loading)** 🔍
- **Conceito**: A aplicação gerencia o cache manualmente
- **Funcionamento**: Lê do cache primeiro, se não encontra vai no DB e atualiza o cache
- **Vantagens**: Controle total sobre o cache, dados carregados sob demanda
- **Desvantagens**: Possível cache miss na primeira consulta
- **Uso ideal**: Dados acessados ocasionalmente

```csharp
var produto = await cacheAsideService.GetProdutoAsync(1);
// Primeira vez: busca no DB e adiciona ao cache
// Próximas vezes: retorna do cache
```

### 2. **Write-Through** 🔄
- **Conceito**: Escreve simultaneamente no cache e no banco de dados
- **Funcionamento**: Garante que cache e DB estejam sempre sincronizados
- **Vantagens**: Consistência garantida entre cache e banco
- **Desvantagens**: Latência maior nas operações de escrita
- **Uso ideal**: Sistemas que exigem consistência forte

```csharp
await writeThroughService.SaveProdutoAsync(produto);
// Salva no DB e cache simultaneamente
```

### 3. **Write-Behind (Write-Back)** ⏰
- **Conceito**: Escreve primeiro no cache, depois persiste no banco de forma assíncrona
- **Funcionamento**: Melhora performance de escrita mas tem risco de perda de dados
- **Vantagens**: Performance de escrita excelente, throughput alto
- **Desvantagens**: Risco de perda de dados, complexidade adicional
- **Uso ideal**: Sistemas com alta frequência de escrita

```csharp
await writeBehindService.SaveProdutoAsync(produto);
// Salva imediatamente no cache, DB atualizado em background
```

### 4. **Read-Through** 📖
- **Conceito**: A aplicação nunca acessa o banco diretamente
- **Funcionamento**: O cache automaticamente busca no banco em caso de miss
- **Vantagens**: Simplifica código da aplicação, cache transparente
- **Desvantagens**: Menos controle sobre as operações de cache
- **Uso ideal**: Aplicações que querem abstração total do cache

```csharp
var produto = await readThroughService.GetProdutoAsync(1);
// Cache gerencia automaticamente DB access
```

### 5. **Refresh-Ahead** 🔄
- **Conceito**: Atualiza o cache proativamente antes da expiração
- **Funcionamento**: Evita cache miss em dados críticos
- **Vantagens**: Elimina cache misses em dados importantes
- **Desvantagens**: Overhead de refresh desnecessários
- **Uso ideal**: Dados críticos com acesso frequente

```csharp
await refreshAheadService.PreloadCriticalDataAsync(new[] { 1, 2, 3 });
// Dados críticos sempre atualizados proativamente
```

### 6. **Full Cache (Cache-First)** 🗄️
- **Conceito**: Carrega todo o dataset ou grandes blocos na inicialização
- **Funcionamento**: Ideal para dados imutáveis ou que mudam pouco
- **Vantagens**: Performance máxima de leitura, índices em memória
- **Desvantagens**: Alto uso de memória, tempo de inicialização
- **Uso ideal**: Dados de referência, configurações, catálogos

```csharp
await fullCacheService.InitializeAsync();
var produtos = await fullCacheService.GetProdutosPorCategoriaAsync("Eletrônicos");
// Todas as consultas vêm do cache, com índices otimizados
```

### 7. **Near Cache** 🌐
- **Conceito**: Cada instância da aplicação mantém uma cópia local do cache
- **Funcionamento**: Usado em sistemas distribuídos para reduzir latência
- **Vantagens**: Latência mínima, reduz carga no cache distribuído
- **Desvantagens**: Uso de memória por instância, sincronização complexa
- **Uso ideal**: Aplicações distribuídas com dados frequentemente acessados

```csharp
var produto = await nearCacheService.GetProdutoAsync(1);
// L1 (local) -> L2 (distribuído) -> L3 (banco)
```

### 8. **Tiered Caching (Multi-Level Cache)** 🎯
- **Conceito**: Usa múltiplos níveis de cache para otimizar performance
- **Funcionamento**: L1 (memória local) → L2 (cache distribuído) → L3 (banco)
- **Vantagens**: Balanceia performance e recursos, métricas detalhadas
- **Desvantagens**: Complexidade de implementação e manutenção
- **Uso ideal**: Sistemas de alta performance com diferentes necessidades de cache

```csharp
var produto = await tieredCacheService.GetProdutoAsync(1);
// Busca hierárquica otimizada com promoção automática
```

## 🛠️ Tecnologias Utilizadas

- **.NET 8.0** - Framework principal
- **Microsoft.Extensions.Caching.Memory** - Cache em memória
- **Microsoft.Extensions.DependencyInjection** - Injeção de dependência
- **Microsoft.Extensions.Logging** - Sistema de logs
- **System.Text.Json** - Serialização JSON

## 🚀 Como Executar

### Pré-requisitos
- .NET 8.0 SDK ou superior
- Visual Studio Code (recomendado)

### Executando o projeto

```bash
# Clone o repositório
git clone <repository-url>
cd CSharp-101/CachePatterns

# Restore dependencies
dotnet restore

# Execute o projeto
dotnet run
```

### Executando via VS Code
1. Abra o projeto no VS Code
2. Use `Ctrl+Shift+P` → "Tasks: Run Task" → "Build and Run Cache Patterns"
3. Ou pressione `F5` para debug

## 📊 Output Esperado

O programa demonstra cada padrão em sequência, mostrando:
- Cache hits e misses
- Operações de escrita e sincronização
- Métricas de performance
- Comportamento de cada padrão

```
=== DEMONSTRAÇÃO DOS PADRÕES DE CACHE ===

🔍 === CACHE-ASIDE PATTERN ===
[CACHE MISS] Produto 1 não encontrado no cache
[DB] Buscando produto ID: 1
[CACHE SET] Produto 1 adicionado ao cache
[CACHE HIT] Produto 1 encontrado no cache
...
```

## 🏗️ Arquitetura do Projeto

```
CachePatterns/
├── Models/                 # Modelos de dados
│   └── Models.cs          # Produto, Usuario, Configuracao
├── Data/                  # Camada de dados
│   └── Repository.cs      # Simulação de banco de dados
├── Patterns/              # Implementações dos padrões
│   ├── CacheAsidePattern.cs
│   ├── WriteThroughPattern.cs
│   ├── WriteBehindPattern.cs
│   ├── ReadThroughPattern.cs
│   ├── RefreshAheadPattern.cs
│   ├── FullCachePattern.cs
│   ├── NearCachePattern.cs
│   └── TieredCachePattern.cs
├── Program.cs             # Demonstrações
└── README.md             # Esta documentação
```

## 🎯 Cenários de Uso

| Padrão | Melhor Para | Evitar Quando |
|--------|-------------|---------------|
| **Cache-Aside** | Dados ocasionais, controle manual | Dados frequentes, muitas escritas |
| **Write-Through** | Consistência crítica | Performance de escrita crítica |
| **Write-Behind** | Alta frequência de escrita | Dados críticos sem tolerância à perda |
| **Read-Through** | Abstração de cache | Controle fino necessário |
| **Refresh-Ahead** | Dados críticos frequentes | Dados raramente acessados |
| **Full Cache** | Dados de referência | Datasets grandes e voláteis |
| **Near Cache** | Sistemas distribuídos | Aplicações single-instance |
| **Tiered Cache** | Sistemas complexos | Aplicações simples |

## 📈 Métricas e Monitoramento

O projeto inclui métricas detalhadas para cada padrão:

```csharp
var metrics = tieredCacheService.GetMetrics();
Console.WriteLine($"Hit Rate L1: {metrics.L1HitRate:F2}%");
Console.WriteLine($"Hit Rate L2: {metrics.L2HitRate:F2}%");
Console.WriteLine($"Cache Hit Rate: {metrics.CacheHitRate:F2}%");
```

## 🔧 Configuração Avançada

### Tempo de Expiração
Cada padrão permite configuração de TTL:
```csharp
private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);
```

### Políticas de Eviction
```csharp
var cacheOptions = new MemoryCacheEntryOptions
{
    Priority = CacheItemPriority.High,
    SlidingExpiration = TimeSpan.FromMinutes(2)
};
```

### Configuração de Memória
```csharp
services.AddMemoryCache(options =>
{
    options.SizeLimit = 1000;
    options.CompactionPercentage = 0.25;
});
```

## 🚨 Considerações de Produção

### Segurança
- Implementar autenticação para operações de cache
- Validar dados antes do cache
- Criptografar dados sensíveis

### Performance
- Monitorar hit rates constantemente
- Ajustar TTL baseado em padrões de uso
- Implementar circuit breakers

### Escalabilidade
- Para produção, usar Redis ou similar para cache distribuído
- Implementar particionamento para grandes datasets
- Considerar cache warming strategies

## 📚 Referências e Estudos Adicionais

- [Microsoft Caching Documentation](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/)
- [Cache-Aside Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside)
- [Redis Best Practices](https://redis.io/documentation)
- [Distributed Caching Patterns](https://martinfowler.com/articles/patterns-of-distributed-systems/)

## 🤝 Contribuindo

Sinta-se à vontade para contribuir com melhorias, correções ou novos padrões de cache!

## 📄 Licença

Este projeto é disponibilizado para fins educacionais e pode ser usado livremente para aprendizado e desenvolvimento.

---

**⭐ Se este projeto foi útil, considere dar uma estrela no repositório!**
