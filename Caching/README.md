# Caching - Coleção de Padrões e Técnicas de Cache

Esta pasta contém exemplos práticos de estratégias de caching para .NET 9, desde padrões fundamentais até implementações avançadas com Redis, MySQL e bibliotecas especializadas.

## 📂 Projetos Disponíveis

### 1. [CacheAside](./CacheAside)
**Padrão Cache-Aside (Lazy Loading)**  
API ASP.NET Core demonstrando o padrão mais comum de cache: verificar cache → buscar no banco em caso de miss → popular cache. Inclui estratégias de invalidação seletiva e TTL diferenciado por tipo de consulta.

**Principais recursos:**
- Cache em memória com `IMemoryCache`
- Endpoints RESTful para produtos
- Logging de hit/miss
- Estratégia de chaves hierárquicas
- Invalidação em operações de escrita

---

### 2. [CacheIncrement](./CacheIncrement)
**Contadores de Alta Performance com Redis + MySQL**  
Demonstra padrão usado por sistemas de larga escala: incrementos atômicos ultra-rápidos no Redis com sincronização periódica para MySQL garantindo durabilidade.

**Principais recursos:**
- Operações `INCR` atômicas no Redis
- Background service para sync automático
- API REST com Swagger
- Monitoramento de status de sincronização
- Configuração de intervalo de persistência

---

### 3. [CachePatterns](./CachePatterns)
**Comparativo de 8 Estratégias de Cache**  
Aplicação console comparando lado a lado diferentes padrões: Cache-Aside, Write-Through, Write-Behind, Read-Through, Refresh-Ahead, Full Cache, Near Cache e Tiered (multi-level).

**Principais recursos:**
- Implementações independentes de cada padrão
- Métricas de hit/miss por estratégia
- Análise de trade-offs (latência vs consistência)
- Casos de uso recomendados para cada padrão

---

### 4. [FusionCache](./FusionCache)
**Cache de Alto Nível com Resiliência**  
Exemplo usando [FusionCache](https://github.com/ZiggyCreatures/FusionCache), biblioteca que combina prevenção de cache stampede, fail-safe, background refresh e suporte multicamadas.

**Principais recursos:**
- API simplificada `GetOrSet`/`GetOrSetAsync`
- Proteção contra thundering herd
- Fail-safe (servir dados stale em falhas)
- Atualização proativa em background
- Documentação comparativa com outras soluções

---

### 5. [RedisConsoleApp](./RedisConsoleApp)
**Operações Essenciais com Redis**  
Aplicação console demonstrando uso de `StackExchange.Redis` com diferentes estruturas de dados: Strings, Hashes, Lists, Sets, Sorted Sets, além de Pub/Sub e rate limiting.

**Principais recursos:**
- Exemplos de todos os tipos de dados Redis
- Implementação de cache-aside genérico
- Rate limiting com sliding window
- Pub/Sub para mensageria
- Boas práticas de conexão e serialização

---

### 6. [RedisMySQLIntegration](./RedisMySQLIntegration)
**Cache Distribuído com Persistência**  
Integração simples mostrando Redis como camada de cache frontal para dados MySQL, reduzindo latência e carga no banco de dados.

**Principais recursos:**
- Leitura com fallback (cache → banco)
- TTL configurável por entidade
- Conexão singleton do Redis
- Padrão de chaves estruturadas

---

## 🎯 Quando Usar Cada Projeto

| Cenário | Projeto Recomendado |
|---------|---------------------|
| Aprender o básico de cache em APIs | **CacheAside** |
| Contadores/métricas de alto tráfego | **CacheIncrement** |
| Entender diferentes padrões e escolher | **CachePatterns** |
| Necessita resiliência e anti-stampede | **FusionCache** |
| Explorar recursos avançados do Redis | **RedisConsoleApp** |
| Cache distribuído simples com banco | **RedisMySQLIntegration** |

## 🚀 Pré-requisitos Gerais

- **.NET 9 SDK** (configurado via `Directory.Build.props` raiz)
- **Redis** (para projetos CacheIncrement, RedisConsoleApp, RedisMySQLIntegration, FusionCache com backend distribuído)
- **MySQL** (para CacheIncrement e RedisMySQLIntegration)

### Instalação Rápida de Dependências (Docker)

```powershell
# Redis
docker run -d --name redis -p 6379:6379 redis

# MySQL
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=senha123 -p 3306:3306 mysql:8
```

## 📚 Conceitos Fundamentais

### Padrões de Cache
- **Cache-Aside (Lazy Loading)**: Aplicação gerencia cache manualmente
- **Write-Through**: Escrita síncrona em cache + banco
- **Write-Behind**: Escrita assíncrona (melhor performance, risco de perda)
- **Read-Through**: Cache intercepta leituras automaticamente
- **Refresh-Ahead**: Renovação proativa antes da expiração

### Métricas Importantes
- **Hit Rate**: % de requisições atendidas pelo cache
- **TTL (Time To Live)**: Tempo de vida do item em cache
- **Eviction**: Política de remoção (LRU, LFU, FIFO)
- **Latência**: Tempo de resposta (cache vs fonte primária)

### Boas Práticas
1. **Chaves estruturadas**: Use prefixos (`product:123`, `user:session:abc`)
2. **TTL apropriado**: Dados estáticos (horas), dados dinâmicos (minutos)
3. **Invalidação seletiva**: Remova apenas chaves afetadas por mudanças
4. **Monitoramento**: Acompanhe hit/miss ratio
5. **Fallback**: Sempre tenha plano B quando cache falhar
6. **Serialização eficiente**: Prefira formatos compactos (MessagePack, Protobuf) para Redis

## 🔧 Comandos Úteis

```powershell
# Executar projeto específico
dotnet run --project "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\Caching\CacheAside\CacheAside.csproj"

# Build de todos os projetos de cache
dotnet build "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\Caching\**\*.csproj"

# Verificar Redis (CLI)
redis-cli ping  # Deve retornar PONG

# Verificar MySQL
mysql -u root -p -e "SHOW DATABASES;"
```

## 📖 Ordem de Estudo Recomendada

1. **CacheAside** - Fundamentos e padrão mais comum
2. **CachePatterns** - Visão comparativa de estratégias
3. **RedisConsoleApp** - Recursos do Redis
4. **RedisMySQLIntegration** - Cache distribuído básico
5. **CacheIncrement** - Padrão de alta performance
6. **FusionCache** - Resiliência e recursos avançados

## 🔗 Referências

- [Microsoft Docs - Caching](https://docs.microsoft.com/aspnet/core/performance/caching/)
- [Redis Patterns](https://redis.io/topics/patterns)
- [Azure Architecture - Cache-Aside](https://docs.microsoft.com/azure/architecture/patterns/cache-aside)
- [FusionCache GitHub](https://github.com/ZiggyCreatures/FusionCache)
- [StackExchange.Redis Docs](https://stackexchange.github.io/StackExchange.Redis/)

---

Todos os projetos seguem convenções do repositório: `.NET 9`, `nullable enable`, `implicit usings`, tipos explícitos para clareza didática.
