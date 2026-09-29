using NSubstitute;
using TestDoublesDemo.Abstractions;
using TestDoublesDemo.Checkout;
using TestDoublesDemo.Domain;
using TestDoublesDemo.Tests.Doubles;

namespace TestDoublesDemo.Tests;

/// <summary>
/// O ponto do exemplo: as duas formas de verificar não custam o mesmo.
///
/// <see cref="PerItemOrderCheckout"/> e <see cref="BatchedOrderCheckout"/> produzem o
/// MESMO resultado de negócio — pedido confirmado, gravado e cliente avisado sobre
/// todos os itens. Mudam apenas em quantas chamadas fazem ao notificador.
/// </summary>
public sealed class StateVersusBehaviorTests
{
    private const string CustomerEmail = "cliente@exemplo.com";

    public static TheoryData<string> Implementations => new TheoryData<string>
    {
        nameof(PerItemOrderCheckout),
        nameof(BatchedOrderCheckout),
    };

    [Theory]
    [MemberData(nameof(Implementations))]
    public void VerificacaoDeEstado_PassaParaAsDuasImplementacoes(string implementation)
    {
        Guid orderId = Guid.NewGuid();
        Order order = BuildOrder(orderId);
        FakeOrderRepository repository = new FakeOrderRepository();
        SpyNotificationSender spy = new SpyNotificationSender();

        IOrderCheckout checkout = Build(implementation, repository, spy);

        CheckoutResult result = checkout.Place(order);

        // Nada aqui olha COMO o servico trabalhou. So o que ficou verdadeiro depois.
        Assert.True(result.IsConfirmed);
        Assert.Equal(OrderStatus.Confirmed, repository.FindById(orderId)?.Status);
        Assert.Equal(1, repository.Count);
        Assert.Contains(spy.Sent, message => message.Body.Contains("TECLADO"));
        Assert.Contains(spy.Sent, message => message.Body.Contains("MOUSE"));
    }

    [Fact]
    public void VerificacaoDeComportamento_DistingueImplementacoesComOMesmoResultado()
    {
        SpyNotificationSender perItemSpy = new SpyNotificationSender();
        SpyNotificationSender batchedSpy = new SpyNotificationSender();

        Build(nameof(PerItemOrderCheckout), new FakeOrderRepository(), perItemSpy)
            .Place(BuildOrder(Guid.NewGuid()));

        Build(nameof(BatchedOrderCheckout), new FakeOrderRepository(), batchedSpy)
            .Place(BuildOrder(Guid.NewGuid()));

        // Mesmo resultado de negocio, contagens de chamada diferentes.
        Assert.Equal(2, perItemSpy.Sent.Count);
        Assert.Single(batchedSpy.Sent);
    }

    [Fact]
    public void MockSobreContagemDeChamadas_FalhaNaRefatoracaoQueNaoMudouOResultado()
    {
        // Este teste foi escrito para a implementacao por item: "espero 2 envios".
        MockNotificationSender mock = new MockNotificationSender(expectedCallCount: 2);

        // A refatoracao agrupou os envios. O cliente continua avisado de tudo, o pedido
        // continua gravado — e a expectativa do mock nao se cumpre mais.
        IOrderCheckout refactored = Build(nameof(BatchedOrderCheckout), new FakeOrderRepository(), mock);

        CheckoutResult result = refactored.Place(BuildOrder(Guid.NewGuid()));

        Assert.True(result.IsConfirmed);

        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(mock.VerifyExpectations);

        Assert.Equal("mock esperava 2 envio(s), recebeu 1", failure.Message);
    }

    [Fact]
    public void NSubstitute_EOMesmoConceitoDeMockSemEscreverODuble()
    {
        // Na pratica ninguem escreve o duble a mao: a biblioteca gera a implementacao e
        // oferece a verificacao. O conceito e o mesmo do MockNotificationSender.
        INotificationSender notifications = Substitute.For<INotificationSender>();
        IInventory inventory = Substitute.For<IInventory>();

        // Configurar retorno e o papel de STUB — mesma biblioteca, outro papel.
        inventory.HasStock(Arg.Any<string>(), Arg.Any<int>()).Returns(true);

        PerItemOrderCheckout checkout = new PerItemOrderCheckout(
            inventory,
            new FakeOrderRepository(),
            notifications,
            new DummyAuditLog());

        checkout.Place(BuildOrder(Guid.NewGuid()));

        notifications.Received(2).Send(CustomerEmail, Arg.Any<string>());
        notifications.Received(1).Send(CustomerEmail, Arg.Is<string>(body => body.Contains("TECLADO")));
    }

    private static Order BuildOrder(Guid id) =>
        new Order(id, CustomerEmail)
            .AddLine("TECLADO", 1, 129.90m)
            .AddLine("MOUSE", 3, 49.90m);

    private static IOrderCheckout Build(
        string implementation,
        IOrderRepository repository,
        INotificationSender notifications)
    {
        StubInventory inventory = new StubInventory(defaultAnswer: true);
        DummyAuditLog auditLog = new DummyAuditLog();

        return implementation == nameof(BatchedOrderCheckout)
            ? new BatchedOrderCheckout(inventory, repository, notifications, auditLog)
            : new PerItemOrderCheckout(inventory, repository, notifications, auditLog);
    }
}
