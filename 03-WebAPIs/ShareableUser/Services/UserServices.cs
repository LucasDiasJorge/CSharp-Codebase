public interface IUsuarioSingletonService
{
    string NomeUsuario { get; set; }
    Guid InstanceId { get; }
    int ContadorAcessos { get; }
    void AcessarUsuario(string requestId);
}

public class UsuarioSingletonService : IUsuarioSingletonService
{
    public string NomeUsuario { get; set; } = "Usuário Padrão";
    public Guid InstanceId { get; } = Guid.NewGuid();
    public int ContadorAcessos { get; private set; }
    
    private readonly object _lock = new object();
    private string _ultimoRequestId;

    public void AcessarUsuario(string requestId)
    {
        lock (_lock)
        {
            ContadorAcessos++;
            
            if (!string.IsNullOrEmpty(_ultimoRequestId) && _ultimoRequestId != requestId)
            {
                Console.WriteLine($"CONCORRÊNCIA DETECTADA! Usuário acessado por: {_ultimoRequestId} e {requestId}");
            }
            
            _ultimoRequestId = requestId;
        }
    }
}