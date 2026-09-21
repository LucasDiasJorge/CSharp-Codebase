using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using TransactionalInboxDemo.Domain;
using TransactionalInboxDemo.Inbox;
using TransactionalInboxDemo.Messaging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InboxDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=inbox-demo.db"));

builder.Services.AddSingleton<ConsumerStats>();
builder.Services.AddHostedService<CreditConsumer>();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    InboxDbContext database = scope.ServiceProvider.GetRequiredService<InboxDbContext>();
    await database.Database.EnsureCreatedAsync();
}

string rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";

// Publica uma mensagem de credito com MessageId novo.
app.MapPost("/messages/credit", async (CreditRequest request) =>
{
    Guid messageId = Guid.NewGuid();
    await PublishAsync(messageId, request.AccountId, request.Amount);

    return Results.Ok(new { messageId, request.AccountId, request.Amount });
});

// Reentrega a MESMA mensagem: e o que um broker faz quando o ack se perde.
app.MapPost("/messages/redeliver", async (RedeliverRequest request) =>
{
    for (int attempt = 0; attempt < request.Times; attempt++)
    {
        await PublishAsync(request.MessageId, request.AccountId, request.Amount);
    }

    return Results.Ok(new
    {
        request.MessageId,
        reentregas = request.Times,
        nota = "Mesmo MessageId. Com inbox o efeito e aplicado uma vez; sem inbox, uma vez por entrega."
    });
});

app.MapGet("/accounts", async (InboxDbContext database) =>
    Results.Ok(await database.Accounts
        .OrderBy(account => account.AccountId)
        .Select(account => new { account.AccountId, account.Balance, account.AppliedCredits })
        .ToListAsync()));

app.MapGet("/inbox", async (InboxDbContext database) =>
    Results.Ok(await database.InboxMessages
        .OrderBy(message => message.Id)
        .Select(message => new { message.MessageId, message.Type, message.ProcessedAt })
        .ToListAsync()));

app.MapGet("/stats", (ConsumerStats stats) => Results.Ok(new
{
    processadas = stats.Processed,
    ignoradasPorDuplicata = stats.Skipped
}));

app.Run();

async Task PublishAsync(Guid messageId, string accountId, decimal amount)
{
    ConnectionFactory factory = new ConnectionFactory { HostName = rabbitHost };

    await using IConnection connection = await factory.CreateConnectionAsync();
    await using IChannel channel = await connection.CreateChannelAsync();

    await CreditConsumer.DeclareTopologyAsync(channel, CancellationToken.None);

    string payload = JsonSerializer.Serialize(new CreditMessage(messageId, accountId, amount),
        new JsonSerializerOptions(JsonSerializerDefaults.Web));

    await channel.BasicPublishAsync(
        exchange: CreditConsumer.ExchangeName,
        routingKey: CreditConsumer.RoutingKey,
        mandatory: false,
        basicProperties: new BasicProperties { MessageId = messageId.ToString(), Persistent = true },
        body: Encoding.UTF8.GetBytes(payload));
}

internal sealed record CreditRequest(string AccountId, decimal Amount);

internal sealed record RedeliverRequest(Guid MessageId, string AccountId, decimal Amount, int Times = 3);
