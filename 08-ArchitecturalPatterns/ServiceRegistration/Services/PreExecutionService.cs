public class PreExecutionService : IPreExecutionService
{
    public void Execute(string message)
    {
        Console.WriteLine($"[Pre-Execution] {message} at {DateTime.UtcNow}");
    }
}