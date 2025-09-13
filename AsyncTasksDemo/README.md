# Async Tasks Demo (.NET)

Demonstrates fundamental `async/await` patterns in .NET:

1. Basic non-blocking delay (`Task.Delay`) — understanding async flow.
2. Parallel API simulations with `Task.WhenAll`.
3. Background logging task with interval + graceful completion.
4. Student tasks (parallel API + timed logging) for practice.

## Why Asynchronous Processing?
Allows responsiveness while waiting on I/O (HTTP calls, DB, timers). Frees threads → better scalability.

## Run
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\AsyncTasksDemo"
dotnet run
```

## Output (excerpt)
```
[Example 1] Basic asynchronous task (non-blocking delay)
Task starting...
Processing...
Task finished.
Task completed!

[Example 2] Parallel API simulation with Task.WhenAll
Fetching API 1...
Fetching API 2...
API 2 Data Retrieved.
API 1 Data Retrieved.
All API calls completed.
```

## Key Methods
| Method | Purpose |
|--------|---------|
| `PerformTaskAsync` | Simulates long-running operation with non-blocking delay |
| `ParallelApiCallsAsync` | Runs two tasks concurrently using `Task.WhenAll` |
| `StartLoggingAsync` | Periodic logging with cancellation support |
| `StudentTaskApisAsync` | Student exercise: parallel API simulation |

## Student Tasks
1. Extend logging to accept dynamic cancellation via key press.
2. Add timeout using `CancellationTokenSource.CancelAfter`.
3. Introduce failure in one API and handle with `Task.WhenAll` + try/catch.

## Extending
- Add progress reporting via `IProgress<T>`.
- Replace `Task.Delay` with `HttpClient` calls.
- Benchmark synchronous vs asynchronous throughput.

## Notes
All delays are deterministic demo values; adjust for experimentation.
