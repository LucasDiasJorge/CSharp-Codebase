using System;
using PortsAndAdapters.Domain;

namespace PortsAndAdapters.app_core;

public class EngineFactory(IEnumerable<IEngine> engines)
{
    private readonly Dictionary<string, IEngine> _engines = CreateEngineAdapter(engines);

    public async Task<EngineExecutionResult> ExecuteCommandAsync(string engineName, string command, CancellationToken cancellationToken)
    {
        if (_engines.TryGetValue(engineName, out IEngine? engine))
        {
            int statusCode = await engine.ExecuteAsync(command, cancellationToken);
            EngineExecutionResult engineExecutionResult = new EngineExecutionResult(engine.Name, command, statusCode, DateTimeOffset.UtcNow);
            return engineExecutionResult;
        }

        throw new ArgumentException($"Engine '{engineName}' not found.");
    }

    public IReadOnlyCollection<string> GetAvailableEngines()
    {
        List<string> availableEngines = _engines.Keys.OrderBy(static key => key, StringComparer.OrdinalIgnoreCase).ToList();
        return availableEngines;
    }

    private static Dictionary<string, IEngine> CreateEngineAdapter(IEnumerable<IEngine> engines)
    {
        Dictionary<string, IEngine> engineDict = new Dictionary<string, IEngine>(StringComparer.OrdinalIgnoreCase);
        foreach (IEngine engine in engines)
        {
            engineDict[engine.Name] = engine;
        }
        return engineDict;
    }
}

public sealed class EngineExecutionResult
{
    public EngineExecutionResult(string engineName, string command, int statusCode, DateTimeOffset executedAtUtc)
    {
        EngineName = engineName;
        Command = command;
        StatusCode = statusCode;
        ExecutedAtUtc = executedAtUtc;
    }

    public string EngineName { get; }
    public string Command { get; }
    public int StatusCode { get; }
    public DateTimeOffset ExecutedAtUtc { get; }
}