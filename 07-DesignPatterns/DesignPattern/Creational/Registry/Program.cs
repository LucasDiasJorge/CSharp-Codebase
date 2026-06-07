using System;
using System.Collections.Generic;

// Interface que todos os handlers devem implementar
public interface IMessageHandler
{
    void Handle(string message);
}

// Implementações concretas
public class EmailHandler : IMessageHandler
{
    public void Handle(string message) =>
        Console.WriteLine($"[EMAIL] Enviando: {message}");
}

public class SmsHandler : IMessageHandler
{
    public void Handle(string message) =>
        Console.WriteLine($"[SMS] Enviando: {message}");
}

public class PushHandler : IMessageHandler
{
    public void Handle(string message) =>
        Console.WriteLine($"[PUSH] Enviando: {message}");
}

// O Registry
public class HandlerRegistry
{
    // Instância única (combina com Singleton)
    private static readonly HandlerRegistry _instance = new();
    public static HandlerRegistry Instance => _instance;

    private readonly Dictionary<string, IMessageHandler> _handlers = new();

    private HandlerRegistry() { }

    public void Register(string key, IMessageHandler handler)
    {
        if (_handlers.ContainsKey(key))
            throw new InvalidOperationException($"Handler '{key}' já registrado.");

        _handlers[key] = handler;
        Console.WriteLine($"Handler '{key}' registrado com sucesso.");
    }

    public IMessageHandler Get(string key)
    {
        if (!_handlers.TryGetValue(key, out var handler))
            throw new KeyNotFoundException($"Handler '{key}' não encontrado.");

        return handler;
    }

    public bool Has(string key) => _handlers.ContainsKey(key);
}

// Uso
class Program
{
    static void Main()
    {
        var registry = HandlerRegistry.Instance;

        // Registrando handlers
        registry.Register("email", new EmailHandler());
        registry.Register("sms",   new SmsHandler());
        registry.Register("push",  new PushHandler());

        Console.WriteLine();

        // Recuperando e usando dinamicamente
        string[] canais = { "email", "sms", "push" };

        foreach (var canal in canais)
        {
            var handler = registry.Get(canal);
            handler.Handle($"Olá via {canal}!");
        }
    }
}