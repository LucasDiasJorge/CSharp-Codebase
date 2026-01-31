namespace Decorator;

/// <summary>
/// Implementação base do notificador
/// Esta é a classe concreta que será decorada
/// </summary>
public class BaseNotifier : INotifier
{
    public void Send(string message)
    {
        Console.WriteLine($"📧 [Base Notification] {message}");
    }

    public string GetDescription()
    {
        return "Base Notification";
    }

    public decimal GetCost()
    {
        return 0.00m; // Notificação básica é gratuita
    }
}
