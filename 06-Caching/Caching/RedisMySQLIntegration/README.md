<!-- README padronizado (versão condensada) -->
# RedisMySQLIntegration (Cache Distribuído com Persistência)

Projeto demonstrativo de integração Redis + MySQL, mostrando padrão de cache distribuído como camada frontal para reduzir latência e carga no banco de dados. Usa `StackExchange.Redis` para cache e `MySql.Data` para persistência.

## 1. Visão Geral
Arquitetura simples em duas camadas:
- **Redis** (camada de leitura rápida): armazena resultados de queries frequentes com TTL
- **MySQL** (fonte de verdade): banco de dados relacional com dados persistentes
- **Fluxo**: tentar Redis → em caso de miss, buscar MySQL → popular Redis com TTL

Benefícios: redução de latência em leituras repetidas, menor carga no banco, escalabilidade horizontal facilitada.

## 2. Objetivos Didáticos
- Demonstrar integração básica Redis + MySQL
- Ilustrar padrão cache-aside em cache distribuído
- Mostrar conexão singleton do Redis (`ConnectionMultiplexer`)
- Evidenciar estratégia de chaves estruturadas
- Preparar para migração de cache local (memória) para distribuído

## 3. Estrutura Principal
```
RedisMySQLIntegration/
  Program.cs (exemplo de uso e fluxo)
  README.md
```
Projeto console minimalista focado em demonstrar a integração.

## 4. Pré-requisitos
- **.NET 9 SDK** (via props global)
- **MySQL** (localhost:3306 ou Docker)
- **Redis** (localhost:6379 ou Docker)

### Subir Infraestrutura (Docker)
```powershell
# MySQL
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=senha123 -p 3306:3306 mysql:8

# Redis
docker run -d --name redis -p 6379:6379 redis
```

## 5. Dependências
```powershell
dotnet add package MySql.Data
dotnet add package StackExchange.Redis
```

## 6. Configuração

### Connection Strings (appsettings.json)
```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;Database=MeuBanco;User=root;Password=senha123;",
    "Redis": "localhost:6379"
  }
}
```

### Criar Banco de Dados
```sql
CREATE DATABASE MeuBanco;
USE MeuBanco;

CREATE TABLE Usuarios (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL
);

INSERT INTO Usuarios (Nome, Email) VALUES 
('João Silva', 'joao@example.com'),
('Maria Santos', 'maria@example.com');
```

## 7. Execução Rápida
```powershell
cd "C:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\Caching\RedisMySQLIntegration"
dotnet restore
dotnet run
```

## 8. Fluxo de Cache (Pseudo)

### Leitura com Cache-Aside
```csharp
public async Task<Usuario?> GetUsuarioAsync(int id)
{
    string cacheKey = string.Format("usuario:{0}", id);
    
    // 1. Tentar cache
    Usuario? cached = await _redisService.GetAsync<Usuario>(cacheKey);
    if (cached != null)
    {
        _logger.LogInformation("Cache HIT: {Key}", cacheKey);
        return cached;
    }
    
    // 2. Cache miss -> buscar MySQL
    _logger.LogInformation("Cache MISS: {Key}", cacheKey);
    Usuario? usuario = await _mysqlRepository.FindByIdAsync(id);
    
    // 3. Popular cache
    if (usuario != null)
    {
        await _redisService.SetAsync(cacheKey, usuario, TimeSpan.FromMinutes(10));
    }
    
    return usuario;
}
```

### Serviço de Cache Redis
```csharp
public class RedisService
{
    private readonly IDatabase _db;
    
    public RedisService(string connectionString)
    {
        ConnectionMultiplexer mux = ConnectionMultiplexer.Connect(connectionString);
        _db = mux.GetDatabase();
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        RedisValue raw = await _db.StringGetAsync(key);
        if (!raw.HasValue) return default(T);
        return JsonSerializer.Deserialize<T>(raw!);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        string json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl);
    }
}
```

## 9. Estratégia de Chaves & TTL

| Tipo de Dado | Formato da Chave | TTL Recomendado |
|--------------|------------------|-----------------|
| Usuário por Id | `usuario:{id}` | 10-30 min |
| Lista de usuários | `usuarios:todos` | 5 min |
| Sessão | `sessao:{sessionId}` | 30-60 min |
| Produto | `produto:{id}` | 1-2 horas |

**Critérios:**
- Dados mais voláteis = TTL menor
- Leituras muito frequentes = TTL maior (com invalidação em escritas)
- Chaves estruturadas facilitam invalidação em lote

## 10. Invalidação em Escritas

### Criar/Atualizar Usuário
```csharp
public async Task UpdateUsuarioAsync(Usuario usuario)
{
    // 1. Atualizar MySQL
    await _repository.UpdateAsync(usuario);
    
    // 2. Invalidar cache específico
    string key = string.Format("usuario:{0}", usuario.Id);
    await _redis.KeyDeleteAsync(key);
    
    // 3. Invalidar listas relacionadas
    await _redis.KeyDeleteAsync("usuarios:todos");
}
```

## 11. Boas Práticas Aplicadas
- **Conexão singleton**: `ConnectionMultiplexer` é thread-safe e reutilizável
- **Serialização**: `System.Text.Json` com DTOs claros
- **Chaves previsíveis**: Padrão `prefixo:identificador`
- **TTL apropriado**: Balanceado por tipo de dado
- **Separação de responsabilidades**: Repository (MySQL) vs CacheService (Redis)
- **Logging**: Hit/miss para observabilidade

## 12. Pontos de Atenção
- Primeira requisição de cada chave sempre paga custo do MySQL (miss)
- Invalidação incorreta = dados obsoletos ou desperdício de memória
- Múltiplas instâncias da aplicação compartilham mesmo Redis (consistência melhorada)
- Falha do Redis deve ter fallback para MySQL (graceful degradation)
- TTL muito longo em dados voláteis = inconsistência

## 13. Extensões Futuras
- **Write-through**: atualizar cache simultaneamente com banco
- **Write-behind**: buffer de escritas assíncronas
- **Circuit breaker**: Polly para falhas sucessivas de MySQL
- **Métricas**: hit ratio, latência Redis vs MySQL
- **Compressão**: para payloads grandes (Gzip, Brotli)
- **Redis Cluster**: alta disponibilidade
- **Invalidação via Pub/Sub**: coordenar entre múltiplas instâncias

## 14. Performance Comparativa

| Operação | Latência Típica |
|----------|-----------------|
| Redis GET | 0.5-2 ms |
| MySQL SELECT simples | 5-20 ms |
| MySQL SELECT complexo | 50-500 ms |

**Ganho**: 10-100x redução de latência em cache hit.

## 15. Troubleshooting
```powershell
# Verificar Redis
redis-cli ping  # Deve retornar PONG
redis-cli KEYS "usuario:*"  # Listar chaves de usuários

# Verificar MySQL
mysql -u root -p -e "SELECT * FROM Usuarios LIMIT 5;"

# Logs da aplicação
# Console exibe hits/misses e operações
```

## 16. Referências
- [StackExchange.Redis Docs](https://stackexchange.github.io/StackExchange.Redis/)
- [MySQL Connector/NET](https://dev.mysql.com/doc/connector-net/en/)
- Cache-Aside Pattern (Azure Architecture)
- Redis Best Practices (redis.io)

## 17. Aprendizados Esperados
Após estudar: compreender integração Redis + MySQL, implementar cache-aside distribuído, gerenciar TTL e invalidação, avaliar trade-offs de consistência eventual.

---
Material versão condensada padronizada com demais projetos do repositório.