using System;
using PortsAndAdapters.Domain;

namespace PortsAndAdapters.app_core;

public abstract class EngineBase : IEngine
{
    public abstract string Name { get; }

    public virtual async Task<int> ExecuteAsync(string command, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken); // Simulate some work
        Console.WriteLine($"Executing command in {Name}: {command} from EngineBase");
        return 0; // Return a result
    }
}