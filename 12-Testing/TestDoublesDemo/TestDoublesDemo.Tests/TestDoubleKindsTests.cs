using TestDoublesDemo.Abstractions;
using TestDoublesDemo.Checkout;
using TestDoublesDemo.Domain;
using TestDoublesDemo.Tests.Doubles;

namespace TestDoublesDemo.Tests;

/// <summary>
/// Um teste por tipo de dublê, sempre sobre o mesmo serviço. O que muda é qual
/// colaborador o teste precisa controlar, e por quê.
/// </summary>
public sealed class TestDoubleKindsTests
{
    private const string CustomerEmail = "cliente@exemplo.com";

    [Fact]
    public void Dummy_PreencheParametroQueOCaminhoNaoUsa()
    {
        // O caminho do pedido vazio nao consulta estoque, nao grava e nao audita.
        // O dummy documenta isso: se o audit log for chamado, ele LANCA e o teste falha.
        Order order = new Order(Guid.NewGuid(), CustomerEmail);

        PerItemOrderCheckout checkout = new PerItemOrderCheckout(
            new StubInventory(defaultAnswer: true),
            new FakeOrderRepository(),
            new SpyNotificationSender(),
            new DummyAuditLog());

        CheckoutResult result = checkout.Place(order);

        Assert.Equal(CheckoutOutcome.EmptyOrder, result.Outcome);
        Assert.Equal(OrderStatus.Rejected, order.Status);
    }

    [Fact]
    public void Stub_ControlaARespostaDaConsultaEComOIssoOCaminho()
    {
        // O stub e o que permite testar "sem estoque" sem precisar de estoque real.
        Order order = new Order(Guid.NewGuid(), CustomerEmail)
            .AddLine("TECLADO", 1, 129.90m)
            .AddLine("MONITOR", 2, 1899.00m);

        StubInventory inventory = new StubInventory(defaultAnswer: true).WithoutStockFor("MONITOR");
        FakeOrderRepository repository = new FakeOrderRepository();
        RecordingAuditLog auditLog = new RecordingAuditLog();

        PerItemOrderCheckout checkout = new PerItemOrderCheckout(
            inventory,
            repository,
            new SpyNotificationSender(),
            auditLog);

        CheckoutResult result = checkout.Place(order);

        Assert.Equal(CheckoutOutcome.OutOfStock, result.Outcome);
        Assert.Contains("MONITOR", result.Reason);
        Assert.Equal(0, repository.Count);
        Assert.Single(auditLog.Entries);
    }

    [Fact]
    public void Spy_GravaAsChamadasEDeixaAVerificacaoParaOTeste()
    {
        Order order = new Order(Guid.NewGuid(), CustomerEmail)
            .AddLine("TECLADO", 1, 129.90m)
            .AddLine("MOUSE", 3, 49.90m);

        SpyNotificationSender spy = new SpyNotificationSender();

        PerItemOrderCheckout checkout = new PerItemOrderCheckout(
            new StubInventory(defaultAnswer: true),
            new FakeOrderRepository(),
            spy,
            new DummyAuditLog());

        checkout.Place(order);

        // O spy nao tinha expectativa nenhuma: quem decide o esperado e o Assert.
        Assert.Equal(2, spy.Sent.Count);
        Assert.All(spy.Sent, message => Assert.Equal(CustomerEmail, message.To));
        Assert.Contains(spy.Sent, message => message.Body.Contains("TECLADO"));
    }

    [Fact]
    public void Mock_CobraAExpectativaQueRecebeuAntesDaExecucao()
    {
        Order order = new Order(Guid.NewGuid(), CustomerEmail)
            .AddLine("TECLADO", 1, 129.90m)
            .AddLine("MOUSE", 3, 49.90m);

        // A expectativa e definida ANTES de executar.
        MockNotificationSender mock = new MockNotificationSender(
            expectedCallCount: 2,
            expectedRecipient: CustomerEmail);

        PerItemOrderCheckout checkout = new PerItemOrderCheckout(
            new StubInventory(defaultAnswer: true),
            new FakeOrderRepository(),
            mock,
            new DummyAuditLog());

        checkout.Place(order);

        // E cobrada pelo proprio duble, nao por um Assert sobre dados.
        mock.VerifyExpectations();
    }

    [Fact]
    public void Fake_TemComportamentoDeVerdadeEPodeSerLidoDeVolta()
    {
        Guid orderId = Guid.NewGuid();
        Order order = new Order(orderId, CustomerEmail).AddLine("TECLADO", 1, 129.90m);

        FakeOrderRepository repository = new FakeOrderRepository();

        PerItemOrderCheckout checkout = new PerItemOrderCheckout(
            new StubInventory(defaultAnswer: true),
            repository,
            new SpyNotificationSender(),
            new DummyAuditLog());

        checkout.Place(order);

        // O fake guarda de verdade: da para ler de volta, como no banco.
        Order? stored = repository.FindById(orderId);

        Assert.NotNull(stored);
        Assert.Equal(OrderStatus.Confirmed, stored.Status);
        Assert.Equal(129.90m, stored.Total);
        Assert.Null(repository.FindById(Guid.NewGuid()));
    }

    /// <summary>
    /// Um spy de audit log, para o teste do stub poder verificar o registro. Fica aqui
    /// porque é usado só neste arquivo.
    /// </summary>
    private sealed class RecordingAuditLog : IAuditLog
    {
        private readonly List<string> _entries = new List<string>();

        public IReadOnlyList<string> Entries => _entries;

        public void Record(string entry) => _entries.Add(entry);
    }
}
