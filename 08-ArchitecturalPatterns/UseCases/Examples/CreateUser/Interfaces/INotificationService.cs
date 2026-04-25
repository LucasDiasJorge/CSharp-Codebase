namespace UseCases.Examples.CreateUser.Interfaces;

/// <summary>
/// Interface para notificações
/// </summary>
public interface INotificationService
{
    Task SendWelcomeEmailAsync(string email, string name, CancellationToken cancellationToken = default);
}
