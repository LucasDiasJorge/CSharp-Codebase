namespace Decorator.Decorators;

/// <summary>
/// Decorador concreto que adiciona prioridade às notificações
/// </summary>
public class PriorityDecorator : NotifierDecorator
{
    private readonly string _priority;

    public PriorityDecorator(INotifier notifier, string priority) : base(notifier)
    {
        _priority = priority;
    }

    public override void Send(string message)
    {
        Console.WriteLine($"⚠️  [PRIORITY: {_priority.ToUpper()}]");
        base.Send(message); // Chama o notificador decorado
    }

    public override string GetDescription()
    {
        return base.GetDescription() + $" + Priority ({_priority})";
    }

    public override decimal GetCost()
    {
        return base.GetCost() + 0.15m; // Prioridade custa $0.15
    }
}
