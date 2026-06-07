using System;
using PortsAndAdapters.app_core;

namespace PortsAndAdapters.usa_domain;

public class EngineUsa : EngineBase
{
    public override string Name => "EngineUsa";

    public override async Task<int> ExecuteAsync(string command, CancellationToken cancellationToken)
    {
        await Task.Delay(400, cancellationToken);
        Console.WriteLine($"Executing command in {Name}: {command} from EngineUsa");
        return 0;
    }
}
