# Poison Looping (Poisoned Loops)

This guide explains, in a practical and concise way, what "Poison Looping" (poisoned loops) is, how to spot it in C# code, simple reproductions, and how to mitigate it.

## What is it
"Poison Loop" is a term used to describe loops that, either by mistake or malicious intent, cause harmful behavior:
- Excessive CPU or memory usage (infinite loops or resource-exhaustion loops)
- Deadlocks, livelocks, or data corruption in concurrent environments
- Training loops that reinforce bad or poisoned data (in ML contexts)
- Constructs that confuse optimizers/compilers and produce unexpected effects

This document focuses on developer-facing scenarios in C# applications.

## Warning signs (how to detect)
- CPU stuck at 90–100% while the application appears idle
- Memory keeps growing without stabilizing (possible leak)
- Loops without a clear exit condition, or whose condition never changes
- Counters/state variables used in the condition are not updated inside the loop
- Use of `while(true)` without cooperative pauses (`await Task.Delay`, `Thread.Sleep`) or a `break`
- In concurrency: multiple threads waiting on resources (deadlock) or spinning without progress (livelock)

Useful tools:
- Logs and metrics: loop runtime, iteration counts, memory usage
- Profilers: `dotnet-trace`, PerfView, Visual Studio Profiler
- Dumps and thread analysis: `dotnet-dump`

## Simple C# examples

### 1) Infinite CPU-draining loop
```csharp
// Symptom: high CPU, unresponsive application
while (true)
{
    // Busy work; no exit condition or pause
}
```

Mitigation:
```csharp
// Use a clear exit condition OR cooperative pauses
var shouldStop = false; // external signal
int maxIterations = 1_000; // defensive limit

for (int i = 0; i < maxIterations && !shouldStop; i++)
{
    // do useful work...
    Thread.Sleep(1); // or await Task.Delay(1) in async context
}
```

### 2) Memory-exhaustion loop
```csharp
var list = new List<byte[]>();
while (true)
{
    // continuously allocates without release
    list.Add(new byte[1024 * 1024]); // +1MB per iteration
}
```

Mitigation:
```csharp
var list = new List<byte[]>();
int max = 10; // defensive limit
for (int i = 0; i < max; i++)
{
    list.Add(new byte[1024 * 1024]);
}
// Release references when no longer needed
list.Clear();
// In real scenarios, prefer streams, reusable buffers or configurable limits
```

### 3) Condition that never changes (logic bug)
```csharp
int x = 0;
while (x < 10)
{
    // forgot to update x
}
```

Mitigation:
```csharp
int x = 0;
while (x < 10)
{
    // ... work
    x++; // update the variable used in the condition
}
```

### 4) Simple livelock (concurrency)
```csharp
// Two threads being "polite" and yielding, so no progress happens
volatile bool resourceInUse = false;

void Work()
{
    while (true)
    {
        if (!resourceInUse)
        {
            resourceInUse = true;
            // do work with the resource
            resourceInUse = false;
            break;
        }
        // The other thread also yields and nobody enters
    }
}
```

Mitigation:
- Use proper primitives: `lock`, `SemaphoreSlim`, `Monitor.TryEnter` with backoff
- Introduce randomized wait/backoff to avoid pathological synchronization

```csharp
var semaphore = new SemaphoreSlim(1, 1);
await semaphore.WaitAsync();
try
{
    // exclusive use of the resource
}
finally
{
    semaphore.Release();
}
```

### 5) Busy-wait without pause
```csharp
while (!conditionMet)
{
    // tight polling loop
}
```

Mitigation:
```csharp
while (!conditionMet)
{
    // re-evaluate with a pause
    await Task.Delay(10); // cooperates with the scheduler
}
```

## Mitigation strategies (checklist)
- Define clear and testable exit conditions
- Defensive limits: maximum iteration counts, maximum time, maximum buffer sizes
- Cooperative pauses in long-running loops: `await Task.Delay`, small `Thread.Sleep`
- Monitoring: runtime metrics, iteration counts, memory, and alarms when thresholds exceed
- Concurrency review: prefer `lock`, `SemaphoreSlim`, `Channel`, `BlockingCollection` instead of ad-hoc synchronization
- Exponential backoff and jitter for retries
- Cancellation support: accept `CancellationToken` in async loops

## How to instrument to detect
- Log loop start/end and execution frequency
- Expose counters: iterations per second, time per iteration
- Sample memory and CPU periodically
- Add configurable limits and emit warnings when exceeded

Simple instrumentation example:
```csharp
var sw = Stopwatch.StartNew();
int iterations = 0;
bool stop = false;

while (!stop && iterations < 10_000)
{
    iterations++;
    if (iterations % 1000 == 0)
    {
        Console.WriteLine($"Iterations: {iterations}, elapsed: {sw.Elapsed}");
        Thread.Sleep(1);
    }
}
```

## Full example: safe async loop pattern
```csharp
public static async Task ProcessQueueAsync(ChannelReader<string> reader, CancellationToken ct)
{
    int processed = 0;
    var maxPerRun = 10_000; // defensive limit

    while (!ct.IsCancellationRequested && processed < maxPerRun)
    {
        while (await reader.WaitToReadAsync(ct))
        {
            while (reader.TryRead(out var item))
            {
                // process item
                processed++;
                if (processed % 1000 == 0)
                {
                    // cooperative pause
                    await Task.Delay(1, ct);
                }
            }
        }
    }
}
```

## Code review checklist
- Does the loop have a clear exit condition?
- Are the condition variables updated inside the loop?
- Are there defensive limits (time, iterations, memory)?
- If waiting, is there a cooperative pause?
- For concurrency, are safe primitives used?
- Are there sufficient logs/metrics to observe behavior?

---

See also the `PoisonLoopingExamples.cs` file in this folder for quick-to-compile examples.
