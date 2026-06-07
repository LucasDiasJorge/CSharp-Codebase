using System;

namespace PortsAndAdapters.Domain;

public interface IEngine
{
    string Name { get; }
    Task<int> ExecuteAsync(string command, CancellationToken cancellationToken);
}