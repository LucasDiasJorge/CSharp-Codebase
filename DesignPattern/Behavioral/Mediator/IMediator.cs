namespace MediatoR;

/// <summary>
/// Interface do Mediator que define o contrato para comunica��o entre componentes
/// </summary>
public interface IMediator
{
    void SendMessage(string message, IUser sender);
    void AddUser(IUser user);
    void RemoveUser(IUser user);
}