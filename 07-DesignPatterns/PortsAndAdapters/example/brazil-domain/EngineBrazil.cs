using System;
using PortsAndAdapters.app_core;

namespace PortsAndAdapters.brazil_domain;

public class EngineBrazil : EngineBase
{
    public override string Name => "EngineBrazil";

    public override async Task<int> ExecuteAsync(string command, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken); // Simulate some work
        Console.WriteLine($"Executing command in {Name}: {command} from EngineBrazil");
        return 0; // Return a result
    }
}
