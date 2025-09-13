using System.Collections.Concurrent;

Console.WriteLine("=== Async Tasks Demo ===\n");

// Entry point orchestrating the demos
await RunAllAsync();

static async Task RunAllAsync()
{
	Console.WriteLine("[Example 1] Basic asynchronous task (non-blocking delay)");
	await PerformTaskAsync();
	Console.WriteLine();

	Console.WriteLine("[Example 2] Parallel API simulation with Task.WhenAll");
	await ParallelApiCallsAsync();
	Console.WriteLine();

	Console.WriteLine("[Student Task 1] Delayed logging system (5 logs, 2s interval)");
	using (CancellationTokenSource cts = new CancellationTokenSource())
	{
		Task logging = StartLoggingAsync(TimeSpan.FromSeconds(2), 5, cts.Token);
		await logging;
	}
	Console.WriteLine();

	Console.WriteLine("[Student Task 2] Simulate two API calls in parallel");
	await StudentTaskApisAsync();
	Console.WriteLine();

	Console.WriteLine("All demos completed.\n");
}

// Example 1: Basic asynchronous method
static async Task PerformTaskAsync()
{
	Console.WriteLine("Task starting...");
	Console.WriteLine("Processing...");
	await Task.Delay(3000); // Simulated non-blocking delay
	Console.WriteLine("Task finished.");
	Console.WriteLine("Task completed!");
}

// Example 2: Two parallel API calls using Task.WhenAll
static async Task ParallelApiCallsAsync()
{
	Console.WriteLine("Starting API calls...");
	Task api1 = FetchApi1Async();
	Task api2 = FetchApi2Async();
	await Task.WhenAll(api1, api2);
	Console.WriteLine("All API calls completed.");
}

static async Task FetchApi1Async()
{
	Console.WriteLine("Fetching API 1...");
	await Task.Delay(3000);
	Console.WriteLine("API 1 Data Retrieved.");
}

static async Task FetchApi2Async()
{
	Console.WriteLine("Fetching API 2...");
	await Task.Delay(2000);
	Console.WriteLine("API 2 Data Retrieved.");
}

// Student Task 1: Background logging every interval
static async Task StartLoggingAsync(TimeSpan interval, int maxMessages, CancellationToken token)
{
	for (int i = 1; i <= maxMessages; i++)
	{
		token.ThrowIfCancellationRequested();
		Console.WriteLine($"[Logger] Message {i} at {DateTime.Now:HH:mm:ss}");
		await Task.Delay(interval, token);
	}
	Console.WriteLine("[Logger] Completed logging cycle.");
}

// Student Task 2: Simulate two API calls in parallel
static async Task StudentTaskApisAsync()
{
	Task<string> t1 = SimulatedApiCallAsync("StudentAPI-1", 2500);
	Task<string> t2 = SimulatedApiCallAsync("StudentAPI-2", 1800);
	string[] results = await Task.WhenAll(t1, t2);
	Console.WriteLine("Student parallel results: " + string.Join(" | ", results));
}

static async Task<string> SimulatedApiCallAsync(string name, int delayMs)
{
	Console.WriteLine($"Calling {name}...");
	await Task.Delay(delayMs);
	return $"{name} OK ({delayMs}ms)";
}
