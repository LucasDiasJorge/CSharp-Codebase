<!-- README padronizado (versão condensada) -->
# RedisMySQLIntegration

Integração simples demonstrando leitura de dados MySQL com cache Redis (StackExchange.Redis) como front para reduzir latência e carga no banco. Tipos explícitos usados para clareza.

## 1. Objetivo
Exibir padrão básico: (a) tentar cache, (b) fallback banco, (c) popular cache com TTL apropriado.

## 2. Dependências Principais
```powershell
dotnet add package MySql.Data
dotnet add package StackExchange.Redis
```

## 3. Execução
1. Subir MySQL e Redis (docker exemplo):
```powershell
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=pass -p 3306:3306 mysql:8
docker run -d --name redis -p 6379:6379 redis
```
2. Ajustar connection strings em `appsettings.json`.
3. Rodar aplicação:
```powershell
dotnet run
```

## 4. Fluxo de Cache (Pseudo)
```csharp
public async Task<Usuario?> GetUsuarioAsync(int id)
{
	string key = string.Format("user:{0}", id);
	Usuario? cached = await _cache.GetAsync<Usuario>(key);
	if (cached != null) return cached;       // HIT
	Usuario? usuario = await _repository.FindAsync(id); // MISS -> DB
	if (usuario != null)
		await _cache.SetAsync(key, usuario, TimeSpan.FromMinutes(10));
	return usuario;
}
```

## 5. Boas Práticas Demonstradas
- Conexão Redis singleton (`ConnectionMultiplexer`).
- TTL configurável por tipo de entidade.
- Chave padronizada (`prefix:id`).
- Separação repositório vs serviço de cache.

## 6. Próximos Passos
- Adicionar invalidação em operações write.
- Introduzir caching de listas/paginação com chaves secundárias.
- Métricas de hit/miss e circuit breaker para MySQL.

## 7. Referências
StackExchange.Redis docs, MySQL Connector/.NET docs.

---
Versão condensada criada a partir de rascunho mínimo original.