<!-- README padronizado (versão condensada) -->
# CacheIncrement (Contadores de Alta Performance)

API ASP.NET Core (.NET 9) demonstrando padrão de contadores de alta escala: incrementos atômicos ultra-rápidos no Redis com sincronização periódica para MySQL, garantindo performance e durabilidade. Padrão usado por sistemas como Facebook, YouTube e Twitter.

## 1. Visão Geral
Arquitetura em duas camadas:
- **Redis** (camada rápida): operações `INCR` atômicas com latência sub-milissegundo
- **MySQL** (camada durável): persistência periódica via background service
- **API REST**: endpoints para incremento, consulta, sincronização manual e monitoramento

Benefícios: throughput massivo (100k+ ops/s no Redis), garantia de durabilidade, recuperação após restart, histórico com timestamps.

## 2. Objetivos Didáticos
- Demonstrar separação entre camada de velocidade e camada de persistência
- Ilustrar operações atômicas do Redis (`INCR`)
- Mostrar background service para sync automático
- Evidenciar monitoramento de status (Redis vs MySQL)
- Preparar para cenários de contadores, rate limiting, analytics em tempo real

## 3. Estrutura Principal
```
CacheIncrement/
  Controllers/ (CounterController)
  Data/ (ApplicationDbContext, migrations)
  Interfaces/ (ICounterService)
  Models/ (Counter, DTOs)
  Services/ (CounterService, CounterSyncBackgroundService)
  Program.cs
  docker-compose.yml (setup de infra)
```

## 4. Pré-requisitos
- **.NET 9 SDK** (via props global)
- **MySQL** (localhost:3306 ou Docker)
- **Redis** (localhost:6379 ou Docker)

### Subir Infraestrutura (Docker)
```powershell
docker run -d --name redis -p 6379:6379 redis
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=senha123 -p 3306:3306 mysql:8
```

Ou usar `docker-compose.yml` do projeto:
```powershell
docker-compose up -d
```

## 5. Configuração

Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CacheIncrementDb;User=root;Password=senha123;",
    "Redis": "localhost:6379"
  },
  "CounterSync": {
    "IntervalMinutes": 1
  }
}
```

Criar banco de dados:
```sql
CREATE DATABASE CacheIncrementDb;
```

## 6. Execução Rápida
```powershell
cd "C:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\Caching\CacheIncrement"
dotnet restore
dotnet run
```
Swagger: `https://localhost:5001` (ou porta atribuída).

## 7. Endpoints Principais

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/counter/{id}/increment?incrementBy=1` | Incrementa contador no Redis |
| GET | `/api/counter/{id}?forceFromDatabase=false` | Obtém valor (padrão: Redis) |
| PUT | `/api/counter/{id}` | Define valor específico |
| GET | `/api/counter/{id}/sync-status` | Compara Redis vs MySQL |
| POST | `/api/counter/{id}/sync` | Força sincronização imediata |
| GET | `/api/counter/mysql/all` | Lista todos os contadores persistidos |
| GET | `/api/counter/health` | Health check |

### Exemplos de Uso
```powershell
# Incrementar contador
curl -X POST "http://localhost:5000/api/counter/page_views/increment"

# Consultar valor
curl "http://localhost:5000/api/counter/page_views"

# Definir valor inicial
curl -X PUT "http://localhost:5000/api/counter/page_views" -H "Content-Type: application/json" -d '{"value": 1000}'

# Verificar sincronização
curl "http://localhost:5000/api/counter/page_views/sync-status"

# Sincronizar manualmente
curl -X POST "http://localhost:5000/api/counter/page_views/sync"
```

## 8. Fluxo de Operação

### Incremento (Pseudo)
```csharp
public async Task<long> IncrementAsync(string counterId, long incrementBy)
{
    long newValue = await _redis.StringIncrementAsync(counterId, incrementBy);
    _logger.LogInformation("Contador {Id} incrementado: {Value}", counterId, newValue);
    return newValue;
}
```

### Background Sync (Periódico)
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await SyncAllCountersAsync();
        await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
    }
}
```

## 9. Boas Práticas Aplicadas
- Operações atômicas do Redis (thread-safe por design)
- Background service registrado como `IHostedService`
- Logging estruturado de sync e incrementos
- Health check para monitoramento
- Separação clara entre camadas rápida e durável
- Configuração externalizada (appsettings)

## 10. Casos de Uso
- Contadores de visualizações de página
- Sistemas de likes/upvotes
- Rate limiting (contagem de requisições)
- Analytics em tempo real
- Tracking de atividade de usuário
- Leaderboards de jogos

## 11. Performance
| Camada | Latência | Throughput | Durabilidade |
|--------|----------|------------|--------------|
| Redis | < 1ms | 100k+ ops/s | Volátil (configurável) |
| MySQL | ~10ms | 1k-10k ops/s | Persistente (ACID) |

Estratégia: aceitar latência eventual (1-5min) entre Redis e MySQL em troca de performance massiva.

## 12. Pontos de Atenção
- Falha antes do sync = perda de contagem (mitigar com Redis persistence `RDB`/`AOF`)
- Múltiplas instâncias da API devem compartilhar mesmo Redis
- Intervalo de sync muito curto = pressão em MySQL; muito longo = risco de perda
- Monitorar diferença Redis vs MySQL para detectar problemas de sync

## 13. Extensões Futuras
- Redis Cluster para alta disponibilidade
- MySQL read replicas para consultas históricas
- Métricas Prometheus (hit rate, sync lag, latência)
- Circuit breaker para falhas de MySQL
- Implementar compaction (merge de contadores antigos)

## 14. Troubleshooting
```powershell
# Verificar Redis
redis-cli ping  # Deve retornar PONG

# Verificar MySQL
mysql -u root -p -e "SHOW DATABASES;"

# Logs da aplicação
# Console exibe status de conexão e sincronizações
```

## 15. Referências
- Redis INCR command (redis.io)
- Background services (.NET IHostedService)
- StackExchange.Redis docs
- High-performance counters patterns

## 16. Aprendizados Esperados
Após estudar: compreender trade-offs entre velocidade e durabilidade, implementar contadores de alta escala, configurar sync periódico, monitorar consistência entre camadas.

---
Material versão condensada padronizada com demais projetos do repositório.
