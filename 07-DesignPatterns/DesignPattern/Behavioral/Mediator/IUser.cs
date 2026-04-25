namespace MediatoR;

/// <summary>
/// Interface para componentes que participam da comunicação mediada
/// </summary>
public interface IUser
{
    string Name { get; }
    void SetMediator(IMediator mediator);
    void SendMessage(string message);
    void ReceiveMessage(string message, string senderName);
}