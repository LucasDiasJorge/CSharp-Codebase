public class PostExecutionService : IPostExecutionService
{
    public void Execute(string message)
    {
        Console.WriteLine($"[Post-Execution] {message} at {DateTime.UtcNow}");
    }
}