using TestDoublesDemo.Domain;

namespace TestDoublesDemo.Abstractions;

/// <summary>
/// Consulta. Colaborador que só responde perguntas — o candidato natural a
/// <b>stub</b>, porque o teste precisa controlar a resposta.
/// </summary>
public interface IInventory
{
    bool HasStock(string sku, int quantity);
}

/// <summary>
/// Comando. Colaborador que só recebe ordens e não devolve nada útil — o candidato
/// natural a <b>spy</b> ou <b>mock</b>, porque o único jeito de saber se o serviço
/// fez o que devia é olhar as chamadas.
/// </summary>
public interface INotificationSender
{
    void Send(string to, string message);
}

/// <summary>
/// Estado. Colaborador com comportamento de verdade — o candidato natural a
/// <b>fake</b>, porque uma implementação em memória substitui o banco sem mentir.
/// </summary>
public interface IOrderRepository
{
    void Save(Order order);

    Order? FindById(Guid id);

    int Count { get; }
}

/// <summary>
/// Colaborador que alguns caminhos do serviço nem tocam — o candidato natural a
/// <b>dummy</b>, quando a assinatura exige um objeto que aquele teste não usa.
/// </summary>
public interface IAuditLog
{
    void Record(string entry);
}
