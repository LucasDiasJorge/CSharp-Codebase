namespace Decorator;

/// <summary>
/// Interface base para notificadores
/// Define o contrato que todos os notificadores devem seguir
/// </summary>
public interface INotifier
{
    void Send(string message);
    string GetDescription();
    decimal GetCost();
}
