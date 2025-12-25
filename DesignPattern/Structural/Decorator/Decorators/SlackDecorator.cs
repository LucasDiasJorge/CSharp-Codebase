namespace Decorator.Decorators;

/// <summary>
/// Decorador concreto que adiciona funcionalidade de envio por Slack
/// </summary>
public class SlackDecorator : NotifierDecorator
{
    private readonly string _channel;

    public SlackDecorator(INotifier notifier, string channel) : base(notifier)
    {
        _channel = channel;
    }

    public override void Send(string message)
    {
        base.Send(message); // Chama o notificador decorado
        SendSlackMessage(message);
    }

    private void SendSlackMessage(string message)
    {
        Console.WriteLine($"💬 [SLACK] Posting to #{_channel}: {message}");
    }

    public override string GetDescription()
    {
        return base.GetDescription() + " + Slack";
    }

    public override decimal GetCost()
    {
        return base.GetCost() + 0.05m; // Slack custa $0.05
    }
}
