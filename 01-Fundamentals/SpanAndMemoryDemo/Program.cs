using System.Threading.Tasks;
using SpanAndMemoryDemo.Demo;

namespace SpanAndMemoryDemo;

public static class Program
{
    public static async Task Main()
    {
        SpanMemoryDemoRunner runner = new();
        await runner.RunAsync();
    }
}
