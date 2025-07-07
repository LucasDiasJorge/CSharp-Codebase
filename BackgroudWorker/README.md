# Background Worker Service

## 📚 Conceitos Abordados

Este projeto demonstra a implementação de serviços em background (Background Services) em .NET:

- **IHostedService**: Interface para serviços hospedados
- **BackgroundService**: Classe base para long-running services
- **Dependency Injection**: Injeção de dependências em serviços
- **Logging**: Sistema de logs estruturado
- **Timer**: Execução periódica de tarefas
- **Graceful Shutdown**: Parada adequada de serviços

## 🎯 Objetivos de Aprendizado

- Criar serviços que executam em background
- Implementar tarefas periódicas e recorrentes
- Gerenciar o ciclo de vida de serviços
- Integrar com o sistema de hosting do .NET
- Implementar logging adequado para monitoramento

## 💡 Conceitos Importantes

### IHostedService
```csharp
public class TimedHostedService : IHostedService, IDisposable
{
    public Task StartAsync(CancellationToken stoppingToken)
    {
        // Inicialização do serviço
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        // Finalização do serviço
    }
}
```

### Timer para Execução Periódica
```csharp
_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
```

### Registro do Serviço
```csharp
builder.Services.AddHostedService<TimedHostedService>();
```

## 🚀 Como Executar

```bash
cd BackgroudWorker
dotnet run
```

## 📖 O que Você Aprenderá

1. **Casos de Uso para Background Services**:
   - Processamento de filas
   - Limpeza de dados antigos
   - Sincronização de dados
   - Monitoramento de sistemas
   - Envio de emails/notificações

2. **Diferença entre IHostedService e BackgroundService**:
   - IHostedService: Controle total do ciclo de vida
   - BackgroundService: Focado em execução contínua

3. **Gestão de Recursos**:
   - Implementação de IDisposable
   - Liberação adequada de recursos
   - Tratamento de cancellation tokens

4. **Integração com DI Container**:
   - Injeção de dependências
   - Singleton vs Scoped services
   - Resolução de dependências

## ⚙️ Tipos de Background Services

### 1. Timed Service (Serviço Temporizado)
Executa tarefas em intervalos regulares:
```csharp
_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
```

### 2. Queue Processing (Processamento de Fila)
Processa itens de uma fila continuamente:
```csharp
while (!stoppingToken.IsCancellationRequested)
{
    var item = await queue.DequeueAsync(stoppingToken);
    await ProcessItem(item);
}
```

### 3. Event-Driven Service (Serviço Orientado a Eventos)
Reage a eventos específicos do sistema.

## 🔍 Pontos de Atenção

- **Resource Management**: Sempre implemente IDisposable quando necessário
- **Cancellation Tokens**: Respeite tokens de cancelamento para shutdown gracioso
- **Exception Handling**: Implemente try/catch para evitar crashes do serviço
- **Logging**: Use logging estruturado para monitoramento
- **Performance**: Evite operações blocking em background services
- **Memory Leaks**: Cuidado com references que não são liberadas

## 🏗️ Padrões de Implementação

### 1. Processamento Batch
```csharp
private async Task ProcessBatch(CancellationToken stoppingToken)
{
    var items = await GetPendingItems();
    foreach (var item in items)
    {
        if (stoppingToken.IsCancellationRequested) break;
        await ProcessItem(item);
    }
}
```

### 2. Retry com Backoff
```csharp
var retryCount = 0;
while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
{
    try
    {
        await ExecuteOperation();
        break;
    }
    catch (Exception ex)
    {
        retryCount++;
        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), stoppingToken);
    }
}
```

## 📚 Recursos Adicionais

- [Background tasks with hosted services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [Worker Services in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/workers)
- [Implement background tasks in microservices](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice)
