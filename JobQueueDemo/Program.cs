using System.Diagnostics;
using System.Threading.Channels;

// ═══════════════════════════════════════════════════════════════════════════════
// 📦 JOBQUEUEDEMO - SISTEMA DIDÁTICO DE FILAS DE PROCESSAMENTO EM C#
// ═══════════════════════════════════════════════════════════════════════════════
//
// OBJETIVO:
// Demonstrar de forma progressiva e didática como implementar e usar
// filas de processamento concorrente usando System.Threading.Channels.
//
// CONCEITOS ABORDADOS:
// • Filas thread-safe com Channel<T>
// • Processamento paralelo com múltiplos workers
// • Operações atômicas com Interlocked
// • Padrão Producer-Consumer
// • Estatísticas e métricas de processamento
// • Cenários do mundo real
//
// ═══════════════════════════════════════════════════════════════════════════════

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.Clear();
        PrintBanner();

        while (true)
        {
            PrintMenu();
            var choice = Console.ReadKey(true).KeyChar;
            Console.Clear();

            try
            {
                switch (choice)
                {
                    case '1':
                        await RunBasicExampleAsync();
                        break;

                    case '2':
                        await RunAdvancedExampleAsync();
                        break;

                    case '3':
                        await RunRealWorldExampleAsync();
                        break;

                    case '0':
                        Console.WriteLine("\n👋 Até logo!\n");
                        return 0;

                    default:
                        Console.WriteLine("\n❌ Opção inválida! Pressione qualquer tecla para continuar...");
                        Console.ReadKey(true);
                        Console.Clear();
                        PrintBanner();
                        continue;
                }

                Console.WriteLine("\n\n✅ Exemplo concluído! Pressione qualquer tecla para voltar ao menu...");
                Console.ReadKey(true);
                Console.Clear();
                PrintBanner();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro: {ex.Message}");
                Console.WriteLine("\nPressione qualquer tecla para continuar...");
                Console.ReadKey(true);
                Console.Clear();
                PrintBanner();
            }
        }
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("""
            ╔═════════════════════════════════════════════════════════════════╗
            ║                                                                 ║
            ║        📦 JOBQUEUEDEMO - FILAS DE PROCESSAMENTO EM C#          ║
            ║                                                                 ║
            ║     Sistema Didático de Aprendizado de Concurrent Queues       ║
            ║                                                                 ║
            ╚═════════════════════════════════════════════════════════════════╝
            
            """);
        Console.ResetColor();
    }

    private static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📚 ESCOLHA UM EXEMPLO:");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.WriteLine("  1️⃣  Exemplo Básico");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("      └─ Conceitos fundamentais de fila e processamento paralelo");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("  2️⃣  Exemplo Avançado");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("      └─ Processamento em larga escala com estatísticas");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("  3️⃣  Exemplo Mundo Real");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("      └─ Sistema de e-commerce com emissão de notas fiscais");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  0️⃣  Sair");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("➤ Digite sua escolha: ");
        Console.ResetColor();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXEMPLO 1: USO BÁSICO DA FILA
    // ═══════════════════════════════════════════════════════════════════════════
    private static async Task RunBasicExampleAsync()
    {
        Console.WriteLine("""
            
            ╔═══════════════════════════════════════════════════════════════╗
            ║              🎓 EXEMPLO 1: USO BÁSICO DA FILA                ║
            ╠═══════════════════════════════════════════════════════════════╣
            ║ Neste exemplo você aprenderá:                                ║
            ║  • Como criar uma fila de processamento                      ║
            ║  • Enfileirar itens para processamento                       ║
            ║  • Processar itens em paralelo com múltiplos workers         ║
            ║  • Entender que a ordem de saída pode diferir da entrada     ║
            ╚═══════════════════════════════════════════════════════════════╝
            
            """);

        // PASSO 1: Criar a fila com 2 workers
        var queue = new InvoiceQueue(workerCount: 2);

        // PASSO 2: Configurar handler para procesamentos
        queue.InvoiceProcessed += (invoice, result) =>
        {
            Console.WriteLine(
                $"    {result.Status} | Ordem #{result.CompletionOrder:D2} | " +
                $"NF {invoice.Id:D3} - R$ {invoice.AmountReais:F2} | {result.ElapsedFormatted}");
        };

        Console.WriteLine("📝 Adicionando 5 notas à fila...\n");

        // PASSO 3: Enfileirar notas com tempos diferentes
        await queue.EnqueueAsync(new Invoice(1, TimeSpan.FromMilliseconds(1000), 0.9, 15000));
        await queue.EnqueueAsync(new Invoice(2, TimeSpan.FromMilliseconds(300), 0.8, 8500));
        await queue.EnqueueAsync(new Invoice(3, TimeSpan.FromMilliseconds(500), 0.95, 12000));
        await queue.EnqueueAsync(new Invoice(4, TimeSpan.FromMilliseconds(200), 0.7, 6000));
        await queue.EnqueueAsync(new Invoice(5, TimeSpan.FromMilliseconds(400), 0.85, 9500));

        Console.WriteLine("\n⏳ Processando...\n");

        // PASSO 4: Drenar a fila
        await queue.DrainAsync();

        Console.WriteLine("""
            
            💡 OBSERVAÇÕES IMPORTANTES:
            ─────────────────────────────────────────────────────────────────
            1. A ORDEM DE CONCLUSÃO não é a mesma da ORDEM DE ENTRADA
               → Notas com tempo menor terminam primeiro
            
            2. Múltiplos workers processam simultaneamente
               → Enquanto Worker 0 processa NF #1 (1000ms),
                 Worker 1 já pode processar NF #2, #3, #4, #5
            
            3. Operações atômicas garantem ordem única de conclusão
            
            4. Cada nota tem probabilidade de sucesso diferente
            
            """);

        await queue.DisposeAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXEMPLO 2: PROCESSAMENTO EM LARGA ESCALA
    // ═══════════════════════════════════════════════════════════════════════════
    private static async Task RunAdvancedExampleAsync()
    {
        Console.WriteLine("""
            
            ╔═══════════════════════════════════════════════════════════════╗
            ║         🚀 EXEMPLO 2: PROCESSAMENTO EM LARGA ESCALA          ║
            ╠═══════════════════════════════════════════════════════════════╣
            ║ Neste exemplo você aprenderá:                                ║
            ║  • Processar grande volume de dados simultaneamente          ║
            ║  • Coletar estatísticas durante o processamento              ║
            ║  • Usar operações atômicas para somas monetárias             ║
            ║  • Calcular métricas de desempenho                           ║
            ╚═══════════════════════════════════════════════════════════════╝
            
            """);

        const int TotalInvoices = 50;
        const int WorkerCount = 6;

        var queue = new InvoiceQueue(WorkerCount);
        var stats = new QueueStatistics();
        var random = new Random();

        queue.InvoiceProcessed += (invoice, result) =>
        {
            stats.RecordProcessing(invoice, result);

            if (result.CompletionOrder % 5 == 0)
            {
                Console.WriteLine(
                    $"    [{result.CompletionOrder:D2}/{TotalInvoices}] {result.Status} | " +
                    $"NF {invoice.Id:D3} - R$ {invoice.AmountReais:F2} | {result.ElapsedFormatted}");
            }
        };

        Console.WriteLine($"📝 Gerando {TotalInvoices} notas fiscais...\n");

        for (int id = 1; id <= TotalInvoices; id++)
        {
            await queue.EnqueueAsync(new Invoice(
                id,
                TimeSpan.FromMilliseconds(random.Next(100, 800)),
                random.NextDouble() * 0.4 + 0.6,
                random.Next(5000, 150000)));
        }

        Console.WriteLine($"\n⏳ Processando com {WorkerCount} workers...\n");

        var stopwatch = Stopwatch.StartNew();
        await queue.DrainAsync();
        stopwatch.Stop();

        Console.WriteLine(stats.GenerateReport());
        Console.WriteLine($"⏱️  Tempo total: {stopwatch.Elapsed.TotalSeconds:F2}s");

        await queue.DisposeAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EXEMPLO 3: SISTEMA DE E-COMMERCE
    // ═══════════════════════════════════════════════════════════════════════════
    private static async Task RunRealWorldExampleAsync()
    {
        Console.WriteLine("""
            
            ╔═══════════════════════════════════════════════════════════════╗
            ║     🏪 EXEMPLO 3: SISTEMA DE E-COMMERCE (MUNDO REAL)         ║
            ╠═══════════════════════════════════════════════════════════════╣
            ║ CENÁRIO:                                                     ║
            ║ Loja online processou 30 vendas e precisa emitir as notas   ║
            ║ O sistema de emissão às vezes falha (timeout, etc)           ║
            ╚═══════════════════════════════════════════════════════════════╝
            
            """);

        var queue = new InvoiceQueue(4);
        var stats = new QueueStatistics();
        var failed = new List<Invoice>();
        var successful = new List<Invoice>();

        queue.InvoiceProcessed += (invoice, result) =>
        {
            stats.RecordProcessing(invoice, result);

            if (result.Success)
            {
                successful.Add(invoice);
                Console.WriteLine(
                    $"    ✅ [#{result.CompletionOrder:D2}] NF {invoice.Id:D3} emitida " +
                    $"| R$ {invoice.AmountReais:F2} | {result.ElapsedFormatted}");
            }
            else
            {
                failed.Add(invoice);
                Console.WriteLine(
                    $"    ❌ [#{result.CompletionOrder:D2}] NF {invoice.Id:D3} FALHOU " +
                    $"| R$ {invoice.AmountReais:F2} | ⚠️  Reprocessar");
            }
        };

        var orders = GenerateEcommerceOrders();
        Console.WriteLine($"📦 Processando {orders.Count} pedidos...\n");

        foreach (var order in orders)
        {
            await queue.EnqueueAsync(order);
        }

        Console.WriteLine("\n⏳ Emitindo notas fiscais...\n");

        var sw = Stopwatch.StartNew();
        await queue.DrainAsync();
        sw.Stop();

        Console.WriteLine("\n" + stats.GenerateReport());
        Console.WriteLine($"\n✅ Emitidas: {successful.Count} | Total: R$ {successful.Sum(i => i.AmountReais):F2}");
        
        if (failed.Count > 0)
        {
            Console.WriteLine($"❌ Falharam: {failed.Count}");
            foreach (var f in failed.Take(5))
            {
                Console.WriteLine($"   → NF #{f.Id:D3} - R$ {f.AmountReais:F2}");
            }
        }

        Console.WriteLine($"\n⏱️  Tempo: {sw.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine($"⚡ Throughput: {orders.Count / sw.Elapsed.TotalSeconds:F1} notas/seg");

        await queue.DisposeAsync();
    }

    private static List<Invoice> GenerateEcommerceOrders()
    {
        var random = new Random(42);
        var orders = new List<Invoice>();
        var categories = new[]
        {
            ("Eletrônicos", 80000, 300000),
            ("Livros", 2000, 15000),
            ("Roupas", 5000, 30000),
            ("Alimentos", 1000, 10000),
            ("Móveis", 50000, 500000)
        };

        for (int i = 1; i <= 30; i++)
        {
            var cat = categories[random.Next(categories.Length)];
            orders.Add(new Invoice(
                i,
                TimeSpan.FromMilliseconds(random.Next(200, 1000)),
                random.NextDouble() * 0.25 + 0.70,
                random.Next(cat.Item2, cat.Item3)));
        }

        return orders;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// MODELOS DE DOMÍNIO
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Representa uma Nota Fiscal a ser emitida
/// </summary>
public readonly record struct Invoice(
    int Id,
    TimeSpan SimulatedWork,
    double SuccessProbability,
    long AmountCents)
{
    public decimal AmountReais => AmountCents / 100.0m;
    public override string ToString() => $"NF #{Id:D3} - R$ {AmountReais:F2}";
}

/// <summary>
/// Resultado do processamento de uma Nota Fiscal
/// </summary>
public readonly record struct InvoiceResult(
    long CompletionOrder,
    bool Success,
    TimeSpan Elapsed,
    DateTimeOffset FinishedAt)
{
    public string Status => Success ? "✅ SUCESSO" : "❌ FALHA";
    public string ElapsedFormatted => $"{Elapsed.TotalMilliseconds:F0}ms";
}

/// <summary>
/// Estatísticas de processamento da fila
/// </summary>
public class QueueStatistics
{
    private long _total, _success, _failure, _amountCents, _timeMs;

    public long TotalProcessed => _total;
    public long SuccessCount => _success;
    public long FailureCount => _failure;
    public decimal TotalAmountReais => _amountCents / 100.0m;
    public double SuccessRate => _total > 0 ? (_success / (double)_total) * 100 : 0;
    public double AvgTimeMs => _total > 0 ? _timeMs / (double)_total : 0;

    public void RecordProcessing(Invoice invoice, InvoiceResult result)
    {
        Interlocked.Increment(ref _total);
        if (result.Success)
        {
            Interlocked.Increment(ref _success);
            Interlocked.Add(ref _amountCents, invoice.AmountCents);
        }
        else
        {
            Interlocked.Increment(ref _failure);
        }
        Interlocked.Add(ref _timeMs, (long)result.Elapsed.TotalMilliseconds);
    }

    public string GenerateReport()
    {
        return $"""
            
            ╔═══════════════════════════════════════════════════════════════╗
            ║           📊 ESTATÍSTICAS DE PROCESSAMENTO                   ║
            ╠═══════════════════════════════════════════════════════════════╣
            ║ Total Processado:      {TotalProcessed,6} notas                    ║
            ║ ✅ Sucessos:            {SuccessCount,6} ({SuccessRate:F1}%)                 ║
            ║ ❌ Falhas:              {FailureCount,6} ({100 - SuccessRate:F1}%)                 ║
            ║                                                               ║
            ║ 💰 Valor Total:         R$ {TotalAmountReais,12:F2}              ║
            ║ ⏱️  Tempo Médio:        {AvgTimeMs,9:F0} ms                      ║
            ╚═══════════════════════════════════════════════════════════════╝
            """;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// FILA DE PROCESSAMENTO
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Fila de processamento de Notas Fiscais usando System.Threading.Channels
/// 
/// CONCEITOS IMPORTANTES:
/// - Channel<T>: Estrutura thread-safe para comunicação produtor-consumidor
/// - Workers: Múltiplas tarefas processando a fila em paralelo
/// - Interlocked: Operações atômicas para evitar race conditions
/// - Events: Notificações quando uma nota é processada
/// </summary>
public sealed class InvoiceQueue : IAsyncDisposable
{
    private readonly Channel<Job> _channel;
    private readonly List<Task> _workers;
    private long _completionSequence;

    public event Action<Invoice, InvoiceResult>? InvoiceProcessed;

    public InvoiceQueue(int workerCount)
    {
        if (workerCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(workerCount));

        // Cria canal ilimitado (unbounded) thread-safe
        _channel = Channel.CreateUnbounded<Job>(new UnboundedChannelOptions
        {
            SingleReader = false,  // Múltiplos workers podem ler
            SingleWriter = false   // Múltiplos produtores podem escrever
        });

        // Inicia os workers
        _workers = Enumerable
            .Range(0, workerCount)
            .Select(id => Task.Run(() => ProcessQueueAsync(id)))
            .ToList();

        Console.WriteLine($"🚀 Fila iniciada com {workerCount} workers paralelos");
    }

    public ValueTask EnqueueAsync(Invoice invoice)
    {
        Console.WriteLine($"  ➕ Enfileirada: {invoice}");
        return _channel.Writer.WriteAsync(new Job(invoice));
    }

    public async Task DrainAsync()
    {
        Console.WriteLine("\n🔒 Finalizando fila...");
        _channel.Writer.TryComplete();
        await Task.WhenAll(_workers);
        Console.WriteLine("✅ Todos os workers finalizaram!");
    }

    private async Task ProcessQueueAsync(int workerId)
    {
        // ReadAllAsync itera sobre itens conforme chegam
        // Loop termina quando canal é fechado e vazio
        await foreach (Job job in _channel.Reader.ReadAllAsync())
        {
            var sw = Stopwatch.StartNew();

            Console.WriteLine($"    🔄 [Worker {workerId}] Processando {job.ToInvoice()}...");

            // Simula processamento
            await Task.Delay(job.SimulatedWork);
            bool success = Random.Shared.NextDouble() <= job.SuccessProbability;

            sw.Stop();

            // Incrementa sequência de forma atômica (thread-safe)
            long order = Interlocked.Increment(ref _completionSequence);

            Invoice invoice = job.ToInvoice();
            var result = new InvoiceResult(order, success, sw.Elapsed, DateTimeOffset.UtcNow);

            InvoiceProcessed?.Invoke(invoice, result);
        }

        Console.WriteLine($"    ⏸️  [Worker {workerId}] Finalizado");
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        try { await Task.WhenAll(_workers); }
        catch { /* Ignora exceções */ }
    }
}

/// <summary>
/// Wrapper interno para transportar dados da Invoice no canal
/// </summary>
internal sealed class Job
{
    public Job(Invoice invoice)
    {
        Id = invoice.Id;
        SimulatedWork = invoice.SimulatedWork;
        SuccessProbability = invoice.SuccessProbability;
        AmountCents = invoice.AmountCents;
    }

    public int Id { get; }
    public TimeSpan SimulatedWork { get; }
    public double SuccessProbability { get; }
    public long AmountCents { get; }

    public Invoice ToInvoice() => new(Id, SimulatedWork, SuccessProbability, AmountCents);
}
