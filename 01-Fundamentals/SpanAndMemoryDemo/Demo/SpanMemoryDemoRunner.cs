using System;
using System.Threading;
using System.Threading.Tasks;
using SpanAndMemoryDemo.Models;
using SpanAndMemoryDemo.Services;

namespace SpanAndMemoryDemo.Demo;

public sealed class SpanMemoryDemoRunner
{
    public async Task RunAsync()
    {
        string rawLine = "MSFT,25,412.35";
        ArraySlicingDemo arrayDemo = new();
        SpanParsingDemo spanDemo = new();
        MemoryProcessingDemo memoryDemo = new(spanDemo);

        Console.WriteLine("Span and Memory Demo");
        Console.WriteLine();

        SliceReport arrayReport = arrayDemo.CreateArraySlice(rawLine);
        PrintSliceReport(arrayReport);

        TradeOrder spanOrder = spanDemo.Parse(rawLine.AsSpan());
        PrintOrder("Parsing com ReadOnlySpan<char>", spanOrder);

        ReadOnlyMemory<char> memory = rawLine.AsMemory();
        TradeOrder memoryOrder = await memoryDemo.ParseAfterAsync(memory, CancellationToken.None);
        PrintOrder("Parsing apos await com ReadOnlyMemory<char>", memoryOrder);

        PrintAllocationComparison(rawLine, arrayDemo, spanDemo);
        PrintUsageLimits();
    }

    private static void PrintSliceReport(SliceReport report)
    {
        Console.WriteLine("== Arrays ==");
        Console.WriteLine($"Origem: {report.Source}");
        Console.WriteLine($"Slice: {report.Slice}");
        Console.WriteLine($"Observacao: {report.Observation}");
        Console.WriteLine();
    }

    private static void PrintOrder(string title, TradeOrder order)
    {
        Console.WriteLine("== " + title + " ==");
        Console.WriteLine($"Ativo: {order.Symbol}");
        Console.WriteLine($"Quantidade: {order.Quantity}");
        Console.WriteLine($"Preco: {order.Price}");
        Console.WriteLine($"Notional: {order.Notional}");
        Console.WriteLine();
    }

    private static void PrintAllocationComparison(
        string rawLine,
        ArraySlicingDemo arrayDemo,
        SpanParsingDemo spanDemo)
    {
        long arrayAllocatedBytes = AllocationMeter.Measure(
            () => arrayDemo.CreateArraySlice(rawLine));

        long spanAllocatedBytes = AllocationMeter.Measure(
            () => spanDemo.Parse(rawLine.AsSpan()));

        Console.WriteLine("== Alocacoes observadas ==");
        Console.WriteLine($"Array slicing: {arrayAllocatedBytes} bytes aproximados nesta thread.");
        Console.WriteLine($"Span parsing: {spanAllocatedBytes} bytes aproximados nesta thread.");
        Console.WriteLine("O parsing ainda aloca a string final do simbolo; as partes intermediarias usam slices.");
        Console.WriteLine();
    }

    private static void PrintUsageLimits()
    {
        Console.WriteLine("== Limites de uso ==");
        Console.WriteLine("Array: pode viver no heap, ser guardado em campos e ter slices que copiam dados.");
        Console.WriteLine("Span<T>: ref struct, fica restrito a escopo seguro e nao deve atravessar await.");
        Console.WriteLine("Memory<T>: pode ser armazenado e passado por APIs async; use .Span apenas no trecho sincronico.");
    }
}
