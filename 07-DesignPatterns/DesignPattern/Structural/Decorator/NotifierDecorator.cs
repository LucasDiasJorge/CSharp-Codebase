namespace Decorator;

/// <summary>
/// Classe abstrata base para todos os decoradores
/// Implementa INotifier e mantém uma referência ao notificador decorado
/// </summary>
public abstract class NotifierDecorator : INotifier
{
    protected INotifier _wrappedNotifier;

    protected NotifierDecorator(INotifier notifier)
    {
        _wrappedNotifier = notifier;
    }

    public virtual void Send(string message)
    {
        _wrappedNotifier.Send(message);
    }

    public virtual string GetDescription()
    {
        return _wrappedNotifier.GetDescription();
    }

    public virtual decimal GetCost()
    {
        return _wrappedNotifier.GetCost();
    }
}
