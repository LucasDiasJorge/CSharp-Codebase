namespace Decorator.Decorators;

/// <summary>
/// Decorador concreto que adiciona funcionalidade de envio por SMS
/// </summary>
public class SmsDecorator : NotifierDecorator
{
    private readonly string _phoneNumber;

    public SmsDecorator(INotifier notifier, string phoneNumber) : base(notifier)
    {
        _phoneNumber = phoneNumber;
    }

    public override void Send(string message)
    {
        base.Send(message); // Chama o notificador decorado
        SendSms(message);
    }

    private void SendSms(string message)
    {
        Console.WriteLine($"📱 [SMS] Sending to {_phoneNumber}: {message}");
    }

    public override string GetDescription()
    {
        return base.GetDescription() + " + SMS";
    }

    public override decimal GetCost()
    {
        return base.GetCost() + 0.25m; // SMS custa $0.25
    }
}
