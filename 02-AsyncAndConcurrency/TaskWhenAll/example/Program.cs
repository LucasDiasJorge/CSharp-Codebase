namespace example;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting tasks...");
        DateTime startTime = DateTime.Now;
        await Example.WaitOneTask();
        await Example.WaitTwoTask();
        await Example.WaitThreeTask();
        DateTime endTime = DateTime.Now;
        Console.WriteLine($"All tasks completed in {(endTime - startTime).TotalSeconds} seconds.");

        Console.WriteLine("Starting tasks with Task.WhenAll...");
        DateTime startTime2 = DateTime.Now;
        await Task.WhenAll(
            Example.WaitOneTask(),
            Example.WaitTwoTask(),
            Example.WaitThreeTask()
        );
        DateTime endTime2 = DateTime.Now;
        Console.WriteLine($"All tasks completed in {(endTime2 - startTime2).TotalSeconds} seconds.");
    }
}

class Example
{
    public static async Task WaitOneTask()
    {
        await Task.Delay(1000);
        Console.WriteLine("One task completed.");
    }

    public static async Task WaitTwoTask()
    {
        await Task.Delay(2000);
        Console.WriteLine("Two tasks completed.");
    }

    public static async Task WaitThreeTask()
    {
        await Task.Delay(3000);
        Console.WriteLine("Three tasks completed.");
    }
}
