# 📦 QueueExample - Estruturas de Dados: Filas (Queue)

## 🎯 Objetivos de Aprendizado

- Entender a **estrutura de dados Queue (Fila)**
- Implementar operações **FIFO (First In, First Out)**
- Usar **Queue<T>** do .NET Collections
- Aplicar filas em **cenários práticos** de programação
- Comparar **Queue vs Stack vs List**
- Implementar **padrões de uso** com filas

## 📚 Conceitos Fundamentais

### O que é uma Queue (Fila)?

Uma **Queue (Fila)** é uma estrutura de dados linear que segue o princípio **FIFO (First In, First Out)** - o primeiro elemento inserido é o primeiro a ser removido, como uma fila de pessoas.

### Operações Principais

- **Enqueue**: Adiciona elemento ao final da fila
- **Dequeue**: Remove e retorna o primeiro elemento
- **Peek**: Visualiza o primeiro elemento sem remover
- **Count**: Número de elementos na fila
- **Clear**: Remove todos os elementos

### Queue vs Outras Estruturas

| Estrutura | Princípio | Inserção | Remoção | Uso Principal |
|-----------|----------|----------|---------|---------------|
| **Queue** | FIFO | Final | Início | Processamento sequencial |
| **Stack** | LIFO | Topo | Topo | Desfazer operações |
| **List** | Indexada | Qualquer posição | Qualquer posição | Acesso aleatório |

## 🏗️ Estrutura do Projeto

```
QueueExample/
├── Program.cs              # Exemplos básicos de Queue
├── Examples/              # Exemplos avançados
│   ├── TaskQueue.cs       # Fila de tarefas
│   ├── PrinterQueue.cs    # Simulação de impressora
│   └── BreadthFirstSearch.cs # BFS com Queue
├── README.md
└── QueueExample.csproj
```

## 💡 Exemplos Práticos

### 1. Operações Básicas

```csharp
public class QueueBasicOperations
{
    public static void DemonstrateBasicOperations()
    {
        // Criar fila
        Queue<string> fila = new Queue<string>();
        
        // Enqueue - Adicionar elementos
        fila.Enqueue("primeiro");
        fila.Enqueue("segundo");
        fila.Enqueue("terceiro");
        fila.Enqueue("quarto");
        fila.Enqueue("quinto");
        
        Console.WriteLine($"Tamanho da fila: {fila.Count}");
        
        // Enumerar sem modificar
        Console.WriteLine("\n📋 Conteúdo da fila:");
        foreach (string item in fila)
        {
            Console.WriteLine($"  - {item}");
        }
        
        // Peek - Ver próximo sem remover
        Console.WriteLine($"\n👀 Próximo item (Peek): {fila.Peek()}");
        Console.WriteLine($"Tamanho após Peek: {fila.Count}");
        
        // Dequeue - Remover e retornar
        Console.WriteLine($"\n🚪 Removendo: {fila.Dequeue()}");
        Console.WriteLine($"🚪 Removendo: {fila.Dequeue()}");
        Console.WriteLine($"Tamanho após Dequeue: {fila.Count}");
        
        // Verificar se contém elemento
        Console.WriteLine($"\n🔍 Contém 'quarto': {fila.Contains("quarto")}");
        
        // Copiar para array
        string[] array = fila.ToArray();
        Console.WriteLine($"\n📄 Array copiado: [{string.Join(", ", array)}]");
        
        // Limpar fila
        fila.Clear();
        Console.WriteLine($"\n🧹 Fila limpa. Tamanho: {fila.Count}");
    }
}
```

### 2. Fila de Tarefas (Task Queue)

```csharp
public class TaskQueue
{
    private readonly Queue<WorkItem> _tasks = new();
    private readonly object _lock = new object();
    
    public void EnqueueTask(string taskName, Func<Task> taskAction)
    {
        lock (_lock)
        {
            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                Name = taskName,
                Action = taskAction,
                QueuedAt = DateTime.UtcNow
            };
            
            _tasks.Enqueue(workItem);
            Console.WriteLine($"📝 Tarefa '{taskName}' adicionada à fila. Total: {_tasks.Count}");
        }
    }
    
    public async Task ProcessTasksAsync()
    {
        while (true)
        {
            WorkItem? workItem = null;
            
            lock (_lock)
            {
                if (_tasks.Count > 0)
                {
                    workItem = _tasks.Dequeue();
                }
            }
            
            if (workItem != null)
            {
                Console.WriteLine($"⚡ Processando '{workItem.Name}'...");
                var startTime = DateTime.UtcNow;
                
                try
                {
                    await workItem.Action();
                    var duration = DateTime.UtcNow - startTime;
                    Console.WriteLine($"✅ '{workItem.Name}' concluída em {duration.TotalMilliseconds:F0}ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro em '{workItem.Name}': {ex.Message}");
                }
            }
            else
            {
                // Sem tarefas, aguarda um pouco
                await Task.Delay(100);
            }
        }
    }
    
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _tasks.Count;
            }
        }
    }
}

public class WorkItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Func<Task> Action { get; set; } = () => Task.CompletedTask;
    public DateTime QueuedAt { get; set; }
}
```

### 3. Simulação de Impressora

```csharp
public class PrinterQueue
{
    private readonly Queue<PrintJob> _printJobs = new();
    private readonly Random _random = new();
    private bool _isProcessing = false;
    
    public void AddPrintJob(string documentName, int pages)
    {
        var job = new PrintJob
        {
            Id = Guid.NewGuid(),
            DocumentName = documentName,
            Pages = pages,
            SubmittedAt = DateTime.UtcNow
        };
        
        _printJobs.Enqueue(job);
        Console.WriteLine($"🖨️  Trabalho '{documentName}' ({pages} páginas) adicionado à fila");
        
        // Inicia processamento se não estiver rodando
        if (!_isProcessing)
        {
            _ = Task.Run(ProcessPrintJobsAsync);
        }
    }
    
    private async Task ProcessPrintJobsAsync()
    {
        _isProcessing = true;
        
        while (_printJobs.Count > 0)
        {
            var job = _printJobs.Dequeue();
            Console.WriteLine($"🖨️  Imprimindo '{job.DocumentName}'... ({_printJobs.Count} na fila)");
            
            // Simula tempo de impressão (100ms por página)
            var printTime = job.Pages * 100;
            await Task.Delay(printTime);
            
            var duration = DateTime.UtcNow - job.SubmittedAt;
            Console.WriteLine($"✅ '{job.DocumentName}' impressa! Tempo total: {duration.TotalSeconds:F1}s");
        }
        
        _isProcessing = false;
        Console.WriteLine("📭 Fila de impressão vazia!");
    }
    
    public void ShowQueueStatus()
    {
        Console.WriteLine($"\n📊 Status da fila: {_printJobs.Count} trabalhos pendentes");
        
        if (_printJobs.Count > 0)
        {
            Console.WriteLine("📋 Trabalhos na fila:");
            var position = 1;
            foreach (var job in _printJobs)
            {
                Console.WriteLine($"  {position}. {job.DocumentName} ({job.Pages} páginas)");
                position++;
            }
        }
    }
}

public class PrintJob
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public int Pages { get; set; }
    public DateTime SubmittedAt { get; set; }
}
```

### 4. Busca em Largura (BFS) com Queue

```csharp
public class BreadthFirstSearch
{
    public static List<int> BFS(Dictionary<int, List<int>> graph, int startNode)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        var result = new List<int>();
        
        queue.Enqueue(startNode);
        visited.Add(startNode);
        
        Console.WriteLine($"🚀 Iniciando BFS a partir do nó {startNode}");
        
        while (queue.Count > 0)
        {
            int currentNode = queue.Dequeue();
            result.Add(currentNode);
            
            Console.WriteLine($"🔍 Visitando nó {currentNode}");
            
            // Adiciona vizinhos não visitados à fila
            if (graph.ContainsKey(currentNode))
            {
                foreach (int neighbor in graph[currentNode])
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        Console.WriteLine($"  ➕ Adicionando vizinho {neighbor} à fila");
                    }
                }
            }
            
            Console.WriteLine($"  📦 Fila atual: [{string.Join(", ", queue)}]");
        }
        
        return result;
    }
    
    public static void DemonstrateBFS()
    {
        // Grafo de exemplo:
        //     1
        //   /   \
        //  2     3
        // / \   / \
        //4   5 6   7
        
        var graph = new Dictionary<int, List<int>>
        {
            { 1, new List<int> { 2, 3 } },
            { 2, new List<int> { 4, 5 } },
            { 3, new List<int> { 6, 7 } },
            { 4, new List<int>() },
            { 5, new List<int>() },
            { 6, new List<int>() },
            { 7, new List<int>() }
        };
        
        var bfsResult = BFS(graph, 1);
        Console.WriteLine($"\n📋 Ordem de visitação BFS: [{string.Join(", ", bfsResult)}]");
    }
}
```

### 5. Queue Thread-Safe com ConcurrentQueue

```csharp
public class ThreadSafeQueueExample
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly CancellationTokenSource _cancellation = new();
    
    public async Task DemonstrateProducerConsumer()
    {
        // Inicia produtor e consumidor em paralelo
        var producerTask = ProducerAsync(_cancellation.Token);
        var consumerTask = ConsumerAsync(_cancellation.Token);
        
        // Executa por 5 segundos
        await Task.Delay(5000);
        _cancellation.Cancel();
        
        await Task.WhenAll(producerTask, consumerTask);
        
        Console.WriteLine($"\n📊 Itens restantes na fila: {_queue.Count}");
    }
    
    private async Task ProducerAsync(CancellationToken cancellationToken)
    {
        int counter = 1;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var item = $"Item-{counter:D3}";
            _queue.Enqueue(item);
            Console.WriteLine($"➕ Produzido: {item} (Total na fila: {_queue.Count})");
            
            counter++;
            await Task.Delay(Random.Shared.Next(200, 500), cancellationToken);
        }
    }
    
    private async Task ConsumerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out string? item))
            {
                Console.WriteLine($"➖ Consumido: {item} (Restante na fila: {_queue.Count})");
                
                // Simula processamento
                await Task.Delay(Random.Shared.Next(100, 300), cancellationToken);
            }
            else
            {
                // Sem itens, aguarda um pouco
                await Task.Delay(50, cancellationToken);
            }
        }
    }
}
```

## 🚀 Configuração e Execução

### 1. Executar Exemplos Básicos

```bash
# Navegar para o diretório
cd QueueExample

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run
```

### 2. Exemplos Interativos

```csharp
public class InteractiveQueueDemo
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🎯 Demonstrações de Queue\n");
        
        // 1. Operações básicas
        Console.WriteLine("1️⃣ Operações Básicas");
        QueueBasicOperations.DemonstrateBasicOperations();
        
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        
        // 2. Fila de tarefas
        Console.WriteLine("2️⃣ Fila de Tarefas");
        await DemonstrateTaskQueue();
        
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        
        // 3. Impressora
        Console.WriteLine("3️⃣ Simulação de Impressora");
        await DemonstratePrinterQueue();
        
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        
        // 4. BFS
        Console.WriteLine("4️⃣ Busca em Largura (BFS)");
        BreadthFirstSearch.DemonstrateBFS();
        
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        
        // 5. Thread-safe
        Console.WriteLine("5️⃣ Producer-Consumer Pattern");
        var threadSafeExample = new ThreadSafeQueueExample();
        await threadSafeExample.DemonstrateProducerConsumer();
    }
    
    private static async Task DemonstrateTaskQueue()
    {
        var taskQueue = new TaskQueue();
        
        // Adiciona algumas tarefas
        taskQueue.EnqueueTask("Download File", () => Task.Delay(1000));
        taskQueue.EnqueueTask("Process Data", () => Task.Delay(800));
        taskQueue.EnqueueTask("Send Email", () => Task.Delay(500));
        taskQueue.EnqueueTask("Generate Report", () => Task.Delay(1200));
        
        // Processa por um tempo limitado
        var processingTask = taskQueue.ProcessTasksAsync();
        await Task.Delay(4000); // Executa por 4 segundos
    }
    
    private static async Task DemonstratePrinterQueue()
    {
        var printer = new PrinterQueue();
        
        // Adiciona trabalhos de impressão
        printer.AddPrintJob("Relatório Mensal", 15);
        printer.AddPrintJob("Apresentação", 8);
        printer.AddPrintJob("Manual do Usuário", 25);
        printer.AddPrintJob("Contrato", 3);
        
        printer.ShowQueueStatus();
        
        // Aguarda processamento
        await Task.Delay(3000);
        
        printer.ShowQueueStatus();
    }
}
```

## 💯 Melhores Práticas

### ✅ Quando Usar Queue

1. **Processamento FIFO**: Quando ordem de processamento importa
2. **Buffer de dados**: Para desacoplar produtores de consumidores
3. **Task scheduling**: Fila de tarefas a serem executadas
4. **BFS algorithms**: Busca em largura em grafos/árvores
5. **Message queues**: Sistemas de mensageria

### ✅ Boas Práticas

1. **Use ConcurrentQueue** para cenários multi-thread
2. **Implemente bounds checking** para filas limitadas
3. **Monitore tamanho** da fila para evitar memory leaks
4. **Use cancellation tokens** em loops de processamento
5. **Trate exceções** adequadamente no processamento

### ❌ Evitar

1. **Acesso direto por índice** (use List<T> se precisar)
2. **Filas muito grandes** sem controle de tamanho
3. **Processamento síncrono** bloqueante
4. **Múltiplas threads** sem sincronização adequada
5. **Loops infinitos** sem cancellation

## 🔍 Complexidade de Operações

| Operação | Complexidade | Descrição |
|----------|-------------|-----------|
| **Enqueue** | O(1) | Adiciona ao final |
| **Dequeue** | O(1) | Remove do início |
| **Peek** | O(1) | Visualiza primeiro elemento |
| **Count** | O(1) | Propriedade cached |
| **Contains** | O(n) | Busca linear |
| **ToArray** | O(n) | Copia todos elementos |

## 📋 Casos de Uso Reais

### 1. Sistema de Cache com Expiração

```csharp
public class ExpiringCache<T>
{
    private readonly Queue<CacheItem<T>> _queue = new();
    private readonly Dictionary<string, T> _cache = new();
    private readonly TimeSpan _ttl;
    
    public ExpiringCache(TimeSpan timeToLive)
    {
        _ttl = timeToLive;
    }
    
    public void Set(string key, T value)
    {
        CleanExpired();
        
        _cache[key] = value;
        _queue.Enqueue(new CacheItem<T>
        {
            Key = key,
            ExpiresAt = DateTime.UtcNow.Add(_ttl)
        });
    }
    
    public bool TryGet(string key, out T? value)
    {
        CleanExpired();
        return _cache.TryGetValue(key, out value);
    }
    
    private void CleanExpired()
    {
        var now = DateTime.UtcNow;
        
        while (_queue.Count > 0 && _queue.Peek().ExpiresAt <= now)
        {
            var expired = _queue.Dequeue();
            _cache.Remove(expired.Key);
        }
    }
}

public class CacheItem<T>
{
    public string Key { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
```

## 📋 Exercícios Práticos

1. **Rate Limiter**: Implemente rate limiting usando Queue
2. **Undo System**: Sistema de desfazer operações
3. **Web Crawler**: Use Queue para URLs a serem processadas
4. **Chat System**: Fila de mensagens em chat
5. **Job Scheduler**: Sistema de agendamento de tarefas

## 🔗 Recursos Adicionais

- [Queue<T> Class Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1)
- [ConcurrentQueue<T> Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1)
- [Choosing Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/selecting-a-collection-class)
- [Data Structures and Algorithms](https://docs.microsoft.com/en-us/dotnet/standard/collections/)

---

💡 **Dica**: Queue é fundamental para implementar padrões como Producer-Consumer, BFS, e task scheduling. Escolha Queue<T> para single-thread e ConcurrentQueue<T> para cenários multi-thread!
