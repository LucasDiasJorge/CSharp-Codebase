using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TransactionalOutboxDemo.Domain;
using TransactionalOutboxDemo.Messaging;
using TransactionalOutboxDemo.Outbox;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=outbox-demo.db"));

builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<OutboxRelay>();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    OrdersDbContext database = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await database.Database.EnsureCreatedAsync();
}

// --- O jeito certo -------------------------------------------------------------------

// Pedido e mensagem commitados JUNTOS, numa transacao explicita. Ou os dois existem,
// ou nenhum — nao ha instante em que o pedido esteja salvo e o evento perdido.
app.MapPost("/orders", async (CreateOrderRequest request, OrdersDbContext database) =>
{
    await using IDbContextTransaction transaction = await database.Database.BeginTransactionAsync();

    Order order = new Order(request.Customer, request.Total);
    database.Orders.Add(order);

    // Este SaveChanges apenas atribui o Id; como esta dentro da transacao, nada virou
    // permanente ainda. Sem a transacao explicita, seriam dois commits separados — e o
    // dual write voltaria por dentro do proprio banco.
    await database.SaveChangesAsync();

    string payload = JsonSerializer.Serialize(new
    {
        orderId = order.Id,
        customer = order.Customer,
        total = order.Total
    });

    database.OutboxMessages.Add(new OutboxMessage("order.created", payload));
    await database.SaveChangesAsync();

    // So aqui os dois se tornam permanentes, de uma vez.
    await transaction.CommitAsync();

    return Results.Ok(new { order.Id, order.Customer, order.Total, via = "outbox" });
});

// --- O jeito errado, para comparar ---------------------------------------------------

// Dual write: grava no banco e depois publica no broker. Sao dois sistemas e duas
// falhas possiveis. Se a publicacao falhar, o pedido fica salvo e o evento some — e o
// cliente recebe erro por algo que, do ponto de vista do banco, deu certo.
app.MapPost("/orders/dual-write", async (CreateOrderRequest request, OrdersDbContext database, RabbitMqPublisher publisher) =>
{
    Order order = new Order(request.Customer, request.Total);
    database.Orders.Add(order);
    await database.SaveChangesAsync();

    string payload = JsonSerializer.Serialize(new { orderId = order.Id, customer = order.Customer, total = order.Total });

    try
    {
        await publisher.PublishAsync(Guid.NewGuid(), "order.created", payload, CancellationToken.None);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Pedido gravado, evento perdido",
            detail: $"O pedido {order.Id} esta no banco, mas o evento nao foi publicado: {ex.Message}. " +
                    "Nenhum consumidor jamais sabera deste pedido.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    return Results.Ok(new { order.Id, order.Customer, order.Total, via = "dual-write" });
});

// --- Inspecao ------------------------------------------------------------------------

app.MapGet("/orders", async (OrdersDbContext database) =>
    Results.Ok(await database.Orders.OrderBy(order => order.Id).ToListAsync()));

app.MapGet("/outbox", async (OrdersDbContext database) =>
{
    List<OutboxMessage> messages = await database.OutboxMessages.OrderBy(message => message.Id).ToListAsync();

    return Results.Ok(messages.Select(message => new
    {
        message.Id,
        message.MessageId,
        message.Type,
        status = message.IsPending ? "pendente" : "publicada",
        message.AttemptCount,
        message.LastError,
        message.OccurredAt,
        message.PublishedAt
    }));
});

app.MapGet("/published", (RabbitMqPublisher publisher) => Results.Ok(publisher.Published));

// Liga e desliga o broker para comparar os dois caminhos sob falha.
app.MapPost("/simulation/broker", (BrokerStateRequest request, RabbitMqPublisher publisher) =>
{
    publisher.SimulatedOutage = request.Down;

    return Results.Ok(new { brokerDown = publisher.SimulatedOutage });
});

app.Run();

internal sealed record CreateOrderRequest(string Customer, decimal Total);

internal sealed record BrokerStateRequest(bool Down);
