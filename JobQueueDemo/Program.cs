using System.Diagnostics;
using System.Threading.Channels;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
    JobQueueDemo demo = new JobQueueDemo();
        await demo.RunAsync();
        return 0;
    }
}

public class JobQueueDemo
{
    private const int WorkerCount = 4;
    private const int TotalInvoices = 15;
    // usamos cents (inteiro) para evitar problemas com ponto flutuante em somas monetárias
    private long _totalIssuedCents;

    public async Task RunAsync()
    {
    InvoiceQueue queue = new InvoiceQueue(WorkerCount);
    queue.InvoiceIssued += OnInvoiceIssued;

        Random random = new Random();

        foreach (int id in Enumerable.Range(1, TotalInvoices))
        {
            TimeSpan simulatedWork = TimeSpan.FromMilliseconds(random.Next(250, 1200));
            double successProbability = random.NextDouble();
            // gerar um valor aleatório entre 10.00 e 1000.00
            int amountCents = random.Next(1000, 100_000);

            await queue.EnqueueAsync(new Invoice(id, simulatedWork, successProbability, amountCents));
        }

        await queue.DrainAsync();

    Console.WriteLine();
    Console.WriteLine("Todas as notas foram processadas.");
    Console.WriteLine($"Total emitido (apenas sucessos): R$ {(_totalIssuedCents / 100.0):F2}");
    }

    private void OnInvoiceIssued(Invoice invoice, InvoiceResult result)
    {
    string status = result.Success ? "SUCESSO" : "FALHA";
        if (result.Success)
        {
            // Incrementa total apenas em casos de sucesso (operacao atomica)
            Interlocked.Add(ref _totalIssuedCents, invoice.AmountCents);
        }

        Console.WriteLine(
            $"[#{result.CompletionOrder:00}] Nota {invoice.Id:00} => {status} em {result.Elapsed.TotalMilliseconds,6:F0} ms | Valor: R$ {(invoice.AmountCents / 100.0):F2} | Chance de sucesso: {invoice.SuccessProbability:P0}");
    }
}

public sealed class InvoiceQueue : IAsyncDisposable
{
    private readonly Channel<Job> _channel;
    private readonly List<Task> _workers;
    private long _completionSequence;

    public InvoiceQueue(int workerCount)
    {
        if (workerCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(workerCount), workerCount, "É necessário pelo menos um worker.");
        }

        _channel = Channel.CreateUnbounded<Job>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

        _workers = Enumerable
            .Range(0, workerCount)
            .Select(_ => Task.Run(ProcessQueueAsync))
            .ToList();
    }


    public event Action<Invoice, InvoiceResult>? InvoiceIssued;

    public ValueTask EnqueueAsync(Invoice invoice) => _channel.Writer.WriteAsync(new Job(invoice));

    public async Task DrainAsync()
    {
        _channel.Writer.TryComplete();
        await Task.WhenAll(_workers);
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (Job job in _channel.Reader.ReadAllAsync())
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            await Task.Delay(job.SimulatedWork);
            bool success = Random.Shared.NextDouble() <= job.SuccessProbability;

            stopwatch.Stop();

            long completionOrder = Interlocked.Increment(ref _completionSequence);

            // reconstruimos o Invoice original a partir do Job (wrapper)
            Invoice invoice = job.ToInvoice();

            InvoiceIssued?.Invoke(
                invoice,
                new InvoiceResult(
                    CompletionOrder: completionOrder,
                    Success: success,
                    Elapsed: stopwatch.Elapsed,
                    FinishedAt: DateTimeOffset.UtcNow));
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();
        try
        {
            await Task.WhenAll(_workers);
        }
        catch (Exception)
        {
            // Ignora exceções ao encerrar workers.
        }
    }
}

// Job é um wrapper interno no canal que carrega os dados necessários para o processamento.
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

public readonly record struct Invoice(int Id, TimeSpan SimulatedWork, double SuccessProbability, long AmountCents);

public readonly record struct InvoiceResult(long CompletionOrder, bool Success, TimeSpan Elapsed, DateTimeOffset FinishedAt);
