# Threading em C#

## 📚 Conceitos Abordados

Este projeto demonstra os fundamentos de threading em C#:

- **Thread Class**: Criação e gerenciamento de threads
- **Thread Lifecycle**: Ciclo de vida das threads
- **Thread Synchronization**: Sincronização entre threads
- **Thread Safety**: Segurança em ambientes multi-thread
- **ThreadPool**: Pool de threads gerenciado
- **Parallel Programming**: Programação paralela
- **Race Conditions**: Condições de corrida

## 🎯 Objetivos de Aprendizado

- Criar e gerenciar threads manualmente
- Entender conceitos de paralelismo vs concorrência
- Implementar sincronização entre threads
- Evitar race conditions e deadlocks
- Usar ThreadPool para eficiência
- Aplicar boas práticas de threading

## 💡 Conceitos Importantes

### Criação de Thread
```csharp
Thread thread = new Thread(Trabalho);
thread.Start();

static void Trabalho()
{
    Console.WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}");
}
```

### Thread com Parâmetros
```csharp
Thread thread = new Thread(TrabalhoComParametro);
thread.Start("Dados para a thread");

static void TrabalhoComParametro(object data)
{
    string dados = (string)data;
    Console.WriteLine($"Processando: {dados}");
}
```

### Thread Join
```csharp
thread.Start();
thread.Join(); // Espera a thread terminar
Console.WriteLine("Thread finalizada");
```

## 🚀 Como Executar

```bash
cd Threads
dotnet run
```

## 📖 O que Você Aprenderá

1. **Threading Basics**:
   - Diferença entre processo e thread
   - Threads foreground vs background
   - Thread states (Running, Sleeping, Blocked, etc.)
   - Context switching

2. **Thread Safety**:
   - Shared data problems
   - Race conditions
   - Atomic operations
   - Memory barriers

3. **Synchronization**:
   - lock statement
   - Monitor class
   - Mutex, Semaphore
   - AutoResetEvent, ManualResetEvent

4. **Performance**:
   - Thread overhead
   - ThreadPool benefits
   - CPU-bound vs I/O-bound tasks

## 🎨 Padrões de Implementação

### 1. Thread Safety com Lock
```csharp
public class ContadorThreadSafe
{
    private int _contador = 0;
    private readonly object _lock = new object();
    
    public void Incrementar()
    {
        lock (_lock)
        {
            _contador++;
        }
    }
    
    public int ObterValor()
    {
        lock (_lock)
        {
            return _contador;
        }
    }
}

class Program
{
    static ContadorThreadSafe contador = new ContadorThreadSafe();
    
    static void Main()
    {
        var threads = new Thread[10];
        
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1000; j++)
                {
                    contador.Incrementar();
                }
            });
            threads[i].Start();
        }
        
        foreach (var thread in threads)
        {
            thread.Join();
        }
        
        Console.WriteLine($"Contador final: {contador.ObterValor()}"); // Deve ser 10000
    }
}
```

### 2. Producer-Consumer com AutoResetEvent
```csharp
public class ProducerConsumer
{
    private readonly Queue<string> _queue = new Queue<string>();
    private readonly AutoResetEvent _itemAvailable = new AutoResetEvent(false);
    private readonly object _queueLock = new object();
    private bool _shutdown = false;
    
    public void Producer()
    {
        for (int i = 0; i < 10; i++)
        {
            var item = $"Item {i}";
            
            lock (_queueLock)
            {
                _queue.Enqueue(item);
                Console.WriteLine($"Produzido: {item}");
            }
            
            _itemAvailable.Set(); // Sinaliza que há item disponível
            Thread.Sleep(500);
        }
        
        _shutdown = true;
        _itemAvailable.Set(); // Acorda o consumer para verificar shutdown
    }
    
    public void Consumer()
    {
        while (!_shutdown || _queue.Count > 0)
        {
            _itemAvailable.WaitOne(); // Espera por item ou shutdown
            
            string item = null;
            lock (_queueLock)
            {
                if (_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                }
            }
            
            if (item != null)
            {
                Console.WriteLine($"Consumido: {item}");
                Thread.Sleep(1000); // Simula processamento
            }
        }
    }
}
```

### 3. Worker Threads com ThreadPool
```csharp
public class WorkerThreadExample
{
    private static int _tasksCompleted = 0;
    private static readonly object _lock = new object();
    
    public static void ExecuteWork()
    {
        const int numberOfTasks = 10;
        var resetEvent = new ManualResetEvent(false);
        
        for (int i = 0; i < numberOfTasks; i++)
        {
            int taskId = i;
            ThreadPool.QueueUserWorkItem(state =>
            {
                DoWork(taskId);
                
                lock (_lock)
                {
                    _tasksCompleted++;
                    if (_tasksCompleted == numberOfTasks)
                    {
                        resetEvent.Set(); // Todas as tarefas completadas
                    }
                }
            });
        }
        
        resetEvent.WaitOne(); // Espera todas as tarefas terminarem
        Console.WriteLine("Todas as tarefas foram completadas!");
    }
    
    private static void DoWork(int taskId)
    {
        Console.WriteLine($"Iniciando tarefa {taskId} na thread {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(Random.Shared.Next(1000, 3000)); // Simula trabalho
        Console.WriteLine($"Tarefa {taskId} completada");
    }
}
```

### 4. Background vs Foreground Threads
```csharp
public class ThreadTypesExample
{
    public static void DemonstrateForegroundThread()
    {
        var foregroundThread = new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Foreground thread: {i}");
                Thread.Sleep(1000);
            }
        });
        
        foregroundThread.IsBackground = false; // Padrão
        foregroundThread.Start();
        
        // Aplicação não termina até foreground threads terminarem
    }
    
    public static void DemonstrateBackgroundThread()
    {
        var backgroundThread = new Thread(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Background thread: {i}");
                Thread.Sleep(1000);
            }
        });
        
        backgroundThread.IsBackground = true;
        backgroundThread.Start();
        
        Thread.Sleep(3000); // Aplicação termina depois de 3 segundos
        Console.WriteLine("Main thread ending - background thread será terminada");
    }
}
```

## 🏗️ Sincronização Avançada

### 1. Semáforo para Controle de Recursos
```csharp
public class ResourceManager
{
    private readonly Semaphore _semaphore;
    private readonly string[] _resources;
    
    public ResourceManager(int resourceCount)
    {
        _semaphore = new Semaphore(resourceCount, resourceCount);
        _resources = new string[resourceCount];
        for (int i = 0; i < resourceCount; i++)
        {
            _resources[i] = $"Resource {i}";
        }
    }
    
    public void UseResource(int threadId)
    {
        _semaphore.WaitOne(); // Adquire um recurso
        
        try
        {
            Console.WriteLine($"Thread {threadId} adquiriu um recurso");
            Thread.Sleep(2000); // Simula uso do recurso
            Console.WriteLine($"Thread {threadId} liberou o recurso");
        }
        finally
        {
            _semaphore.Release(); // Libera o recurso
        }
    }
}
```

### 2. Reader-Writer Lock
```csharp
public class ReadWriteExample
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private Dictionary<string, string> _data = new Dictionary<string, string>();
    
    public string Read(string key)
    {
        _lock.EnterReadLock();
        try
        {
            _data.TryGetValue(key, out string value);
            Console.WriteLine($"Read operation by thread {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(100); // Simula leitura
            return value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    public void Write(string key, string value)
    {
        _lock.EnterWriteLock();
        try
        {
            _data[key] = value;
            Console.WriteLine($"Write operation by thread {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(200); // Simula escrita
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
```

## 🔍 Pontos de Atenção

### Deadlock Prevention
```csharp
// ❌ Pode causar deadlock
object lock1 = new object();
object lock2 = new object();

// Thread 1
lock (lock1)
{
    lock (lock2) { /* trabalho */ }
}

// Thread 2
lock (lock2)
{
    lock (lock1) { /* trabalho */ } // Deadlock!
}

// ✅ Sempre adquirir locks na mesma ordem
lock (lock1)
{
    lock (lock2) { /* trabalho */ }
}
```

### Resource Management
```csharp
// ✅ Sempre limpe recursos
public class ThreadSafeResource : IDisposable
{
    private readonly Mutex _mutex = new Mutex();
    
    public void DoWork()
    {
        _mutex.WaitOne();
        try
        {
            // Trabalho crítico
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }
    
    public void Dispose()
    {
        _mutex?.Dispose();
    }
}
```

### Performance Considerations
```csharp
// ✅ Prefira ThreadPool para trabalhos curtos
ThreadPool.QueueUserWorkItem(DoShortWork);

// ✅ Crie threads manualmente apenas para trabalhos longos
var longRunningThread = new Thread(DoLongWork);
longRunningThread.Start();

// ✅ Use cancellation tokens
public void DoWork(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        // Trabalho
    }
}
```

## 🚀 Evolução para Async/Await

### Threading Tradicional vs Async/Await
```csharp
// Threading tradicional - bloqueia thread
public void TraditionalApproach()
{
    var thread = new Thread(() =>
    {
        var data = GetDataFromNetwork(); // Bloqueia thread
        ProcessData(data);
    });
    thread.Start();
}

// Abordagem moderna - async/await
public async Task ModernApproach()
{
    var data = await GetDataFromNetworkAsync(); // Não bloqueia thread
    ProcessData(data);
}
```

## 📚 Recursos Adicionais

- [Threading in C#](https://docs.microsoft.com/en-us/dotnet/standard/threading/)
- [Thread Safety](https://docs.microsoft.com/en-us/dotnet/standard/threading/thread-safety)
- [Task Parallel Library](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/)
