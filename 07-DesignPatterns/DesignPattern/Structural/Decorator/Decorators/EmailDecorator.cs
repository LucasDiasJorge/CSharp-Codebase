namespace Decorator.Decorators;

/// <summary>
/// Decorador concreto que adiciona funcionalidade de envio por Email
/// </summary>
public class EmailDecorator : NotifierDecorator
{
    private readonly string _emailAddress;

    public EmailDecorator(INotifier notifier, string emailAddress) : base(notifier)
    {
        _emailAddress = emailAddress;
    }

    public override void Send(string message)
    {
        base.Send(message); // Chama o notificador decorado
        SendEmail(message);
    }

    private void SendEmail(string message)
    {
        Console.WriteLine($"📧 [EMAIL] Sending to {_emailAddress}: {message}");
    }

    public override string GetDescription()
    {
        return base.GetDescription() + " + Email";
    }

    public override decimal GetCost()
    {
        return base.GetCost() + 0.10m; // Email custa $0.10
    }
}
