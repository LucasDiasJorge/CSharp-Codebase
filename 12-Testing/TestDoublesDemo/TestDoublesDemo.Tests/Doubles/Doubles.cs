using TestDoublesDemo.Abstractions;
using TestDoublesDemo.Domain;

namespace TestDoublesDemo.Tests.Doubles;

/// <summary>
/// <b>DUMMY</b> — existe só para preencher um parâmetro. Não responde nada útil e não
/// registra nada.
///
/// Lançar exceção em vez de ficar calado é o que transforma o dummy em documentação
/// executável: se o caminho testado tocar nele, o teste falha e diz que a premissa
/// ("este caminho não audita") estava errada.
/// </summary>
public sealed class DummyAuditLog : IAuditLog
{
    public void Record(string entry) =>
        throw new InvalidOperationException(
            $"dummy nao deveria ser chamado, mas recebeu: '{entry}'");
}

/// <summary>
/// <b>STUB</b> — devolve respostas prontas para que o teste controle o caminho.
/// Não verifica nada; só alimenta a decisão de quem está sendo testado.
/// </summary>
public sealed class StubInventory : IInventory
{
    private readonly bool _defaultAnswer;
    private readonly Dictionary<string, bool> _answersBySku;

    public StubInventory(bool defaultAnswer)
    {
        _defaultAnswer = defaultAnswer;
        _answersBySku = new Dictionary<string, bool>();
    }

    public StubInventory WithoutStockFor(string sku)
    {
        _answersBySku[sku] = false;

        return this;
    }

    public bool HasStock(string sku, int quantity) =>
        _answersBySku.TryGetValue(sku, out bool answer) ? answer : _defaultAnswer;
}

public sealed record SentMessage(string To, string Body);

/// <summary>
/// <b>SPY</b> — registra o que recebeu e deixa a verificação para o teste. Não tem
/// expectativa própria: quem decide o que era esperado é o <c>Assert</c>, depois.
///
/// É a diferença em relação ao mock: o spy grava, o mock cobra.
/// </summary>
public sealed class SpyNotificationSender : INotificationSender
{
    private readonly List<SentMessage> _sent = new List<SentMessage>();

    public IReadOnlyList<SentMessage> Sent => _sent;

    public void Send(string to, string message) => _sent.Add(new SentMessage(to, message));
}

/// <summary>
/// <b>MOCK</b> — recebe a expectativa ANTES da execução e falha por conta própria
/// quando ela não é cumprida. A verificação está dentro do dublê, não no teste.
///
/// É o que torna o mock o dublê mais acoplado à implementação: a expectativa é sobre
/// COMO o serviço trabalha.
/// </summary>
public sealed class MockNotificationSender : INotificationSender
{
    private readonly int _expectedCallCount;
    private readonly string? _expectedRecipient;
    private readonly List<SentMessage> _received = new List<SentMessage>();

    public MockNotificationSender(int expectedCallCount, string? expectedRecipient = null)
    {
        _expectedCallCount = expectedCallCount;
        _expectedRecipient = expectedRecipient;
    }

    public void Send(string to, string message)
    {
        if (_expectedRecipient is not null && to != _expectedRecipient)
        {
            throw new InvalidOperationException(
                $"mock esperava envio para '{_expectedRecipient}', recebeu '{to}'");
        }

        _received.Add(new SentMessage(to, message));
    }

    /// <summary>
    /// Cobra a expectativa. Um mock de verdade falha o teste sozinho — é por isso que
    /// esta chamada existe e precisa acontecer no fim do teste.
    /// </summary>
    public void VerifyExpectations()
    {
        if (_received.Count != _expectedCallCount)
        {
            throw new InvalidOperationException(
                $"mock esperava {_expectedCallCount} envio(s), recebeu {_received.Count}");
        }
    }
}

/// <summary>
/// <b>FAKE</b> — implementação de verdade, simplificada. Guarda, encontra, conta.
/// Um teste pode usá-la como usaria o banco, inclusive lendo de volta o que gravou.
///
/// É o único dublê com comportamento próprio — e por isso o único que pode ter bug.
/// </summary>
public sealed class FakeOrderRepository : IOrderRepository
{
    private readonly Dictionary<Guid, Order> _orders = new Dictionary<Guid, Order>();

    public int Count => _orders.Count;

    public void Save(Order order) => _orders[order.Id] = order;

    public Order? FindById(Guid id) => _orders.TryGetValue(id, out Order? order) ? order : null;
}
