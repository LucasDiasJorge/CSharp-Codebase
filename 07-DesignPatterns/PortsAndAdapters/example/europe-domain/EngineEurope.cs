using System;
using PortsAndAdapters.app_core;

namespace PortsAndAdapters.europe_domain;

public class EngineEurope : EngineBase
{
    public override string Name => "EngineEurope";

    public override async Task<int> ExecuteAsync(string command, CancellationToken cancellationToken)
    {
        await Task.Delay(700, cancellationToken);
        Console.WriteLine($"Executing command in {Name}: {command} from EngineEurope");
        return 0;
    }
}
