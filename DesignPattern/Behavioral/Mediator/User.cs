namespace MediatoR;

/// <summary>
/// Componente concreto que representa um usu�rio no chat
/// Comunica-se com outros usu�rios atrav�s do mediator
/// </summary>
public class User : IUser
{
    private IMediator? _mediator;
    
    public string Name { get; }

    public User(string name)
    {
        Name = name;
    }

    public void SetMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public void SendMessage(string message)
    {
        if (_mediator == null)
        {
            Console.WriteLine($"[Erro] {Name} n�o est� conectado a nenhuma sala de chat");
            return;
        }

        _mediator.SendMessage(message, this);
    }

    public void ReceiveMessage(string message, string senderName)
    {
        Console.WriteLine($"  > {Name} recebeu: '{message}' de {senderName}");
    }
}