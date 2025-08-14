namespace MediatoR;

/// <summary>
/// Implementação concreta do Mediator para uma sala de chat
/// Centraliza toda a comunicação entre usuários
/// </summary>
public class ChatRoomMediator : IMediator
{
    private readonly List<IUser> _users;

    public ChatRoomMediator()
    {
        _users = new List<IUser>();
    }

    public void AddUser(IUser user)
    {
        _users.Add(user);
        user.SetMediator(this);
        Console.WriteLine($"[Sistema] {user.Name} entrou na sala de chat");
    }

    public void RemoveUser(IUser user)
    {
        _users.Remove(user);
        Console.WriteLine($"[Sistema] {user.Name} saiu da sala de chat");
    }

    public void SendMessage(string message, IUser sender)
    {
        Console.WriteLine($"[Chat] {sender.Name}: {message}");
        
        // Enviar mensagem para todos os outros usuários (exceto o remetente)
        foreach (var user in _users)
        {
            if (user != sender)
            {
                user.ReceiveMessage(message, sender.Name);
            }
        }
    }
}