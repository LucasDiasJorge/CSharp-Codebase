using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TransactionalInboxDemo.Domain;
using TransactionalInboxDemo.Inbox;

namespace TransactionalInboxDemo.Messaging;

/// <summary>
/// Consumidor com inbox. Processa cada mensagem no máximo uma vez, mesmo recebendo-a
/// várias vezes — que é o que vai acontecer, porque entrega pelo menos uma vez é a
/// garantia normal de qualquer broker.
/// </summary>
public sealed class CreditConsumer : BackgroundService
{
    public const string ExchangeName = "inbox-demo";
    public const string GuardedQueue = "inbox-demo.guarded";
    public const string NaiveQueue = "inbox-demo.naive";
    public const string RoutingKey = "credit";

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConsumerStats _stats;
    private readonly string _hostName;
    private readonly ILogger<CreditConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public CreditConsumer(IServiceScopeFactory scopeFactory, ConsumerStats stats, IConfiguration configuration, ILogger<CreditConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _stats = stats;
        _hostName = configuration["RabbitMq:Host"] ?? "localhost";
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ConnectionFactory factory = new ConnectionFactory { HostName = _hostName };
        _connection = await factory.CreateConnectionAsync(stoppingToken).ConfigureAwait(false);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken).ConfigureAwait(false);

        await DeclareTopologyAsync(_channel, stoppingToken).ConfigureAwait(false);
        await _channel.BasicQosAsync(0, prefetchCount: 1, global: false, cancellationToken: stoppingToken).ConfigureAwait(false);

        AsyncEventingBasicConsumer guarded = new AsyncEventingBasicConsumer(_channel);
        guarded.ReceivedAsync += (_, args) => HandleAsync(args, useInbox: true);

        AsyncEventingBasicConsumer naive = new AsyncEventingBasicConsumer(_channel);
        naive.ReceivedAsync += (_, args) => HandleAsync(args, useInbox: false);

        await _channel.BasicConsumeAsync(GuardedQueue, autoAck: false, consumer: guarded, cancellationToken: stoppingToken).ConfigureAwait(false);
        await _channel.BasicConsumeAsync(NaiveQueue, autoAck: false, consumer: naive, cancellationToken: stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("Consumindo {ComInbox} (com inbox) e {SemInbox} (sem inbox).", GuardedQueue, NaiveQueue);

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default).ConfigureAwait(false);
    }

    public static async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken).ConfigureAwait(false);

        // Fanout: a MESMA mensagem chega nas duas filas, para o contraste ser exato.
        await channel.QueueDeclareAsync(GuardedQueue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.QueueDeclareAsync(NaiveQueue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken).ConfigureAwait(false);

        await channel.QueueBindAsync(GuardedQueue, ExchangeName, RoutingKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.QueueBindAsync(NaiveQueue, ExchangeName, RoutingKey, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleAsync(BasicDeliverEventArgs args, bool useInbox)
    {
        if (_channel is null)
        {
            return;
        }

        string body = Encoding.UTF8.GetString(args.Body.ToArray());
        CreditMessage? message = JsonSerializer.Deserialize<CreditMessage>(body, JsonOptions);

        if (message is null)
        {
            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);

            return;
        }

        using IServiceScope scope = _scopeFactory.CreateScope();
        InboxDbContext database = scope.ServiceProvider.GetRequiredService<InboxDbContext>();

        if (useInbox)
        {
            await ProcessWithInboxAsync(database, message).ConfigureAwait(false);
        }
        else
        {
            await ProcessWithoutInboxAsync(database, message).ConfigureAwait(false);
        }

        await _channel.BasicAckAsync(args.DeliveryTag, multiple: false).ConfigureAwait(false);
    }

    /// <summary>
    /// O caminho correto. A marca na inbox e o efeito no domínio são commitados na
    /// MESMA transação: ou os dois acontecem, ou nenhum.
    /// </summary>
    private async Task ProcessWithInboxAsync(InboxDbContext database, CreditMessage message)
    {
        await using IDbContextTransaction transaction = await database.Database.BeginTransactionAsync();

        bool alreadyProcessed = await database.InboxMessages
            .AnyAsync(inbox => inbox.MessageId == message.MessageId);

        if (alreadyProcessed)
        {
            _stats.RecordSkipped();
            _logger.LogInformation("Com inbox: mensagem {MensagemId} ja processada, ignorada.", message.MessageId);

            await transaction.RollbackAsync();

            return;
        }

        Account account = await GetOrCreateAsync(database, message.AccountId);
        account.Credit(message.Amount);

        // A marca entra na mesma unidade de trabalho do credito. Grava-la depois, em
        // outra transacao, deixaria uma janela em que o credito ja foi aplicado mas a
        // mensagem ainda parece nova — e uma reentrega creditaria de novo.
        database.InboxMessages.Add(new InboxMessage(message.MessageId, nameof(CreditMessage)));

        await database.SaveChangesAsync();
        await transaction.CommitAsync();

        _stats.RecordProcessed();
        _logger.LogInformation("Com inbox: {Valor:F2} creditado em {Conta} (mensagem {MensagemId}).", message.Amount, message.AccountId, message.MessageId);
    }

    /// <summary>
    /// O caminho ingênuo, mantido para comparação: aplica o crédito sem verificar se já
    /// processou aquela mensagem. Uma reentrega credita de novo.
    /// </summary>
    private async Task ProcessWithoutInboxAsync(InboxDbContext database, CreditMessage message)
    {
        Account account = await GetOrCreateAsync(database, message.AccountId + "-sem-inbox");
        account.Credit(message.Amount);

        await database.SaveChangesAsync();

        _logger.LogWarning("Sem inbox: {Valor:F2} creditado em {Conta} sem checar duplicata.", message.Amount, account.AccountId);
    }

    private static async Task<Account> GetOrCreateAsync(InboxDbContext database, string accountId)
    {
        Account? account = await database.Accounts.FirstOrDefaultAsync(item => item.AccountId == accountId);

        if (account is null)
        {
            account = new Account(accountId, 0m);
            database.Accounts.Add(account);
        }

        return account;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync().ConfigureAwait(false);
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}

public sealed record CreditMessage(Guid MessageId, string AccountId, decimal Amount);

/// <summary>Contadores do consumidor, só para o exemplo.</summary>
public sealed class ConsumerStats
{
    private int _processed;
    private int _skipped;

    public int Processed => Volatile.Read(ref _processed);

    public int Skipped => Volatile.Read(ref _skipped);

    public void RecordProcessed() => Interlocked.Increment(ref _processed);

    public void RecordSkipped() => Interlocked.Increment(ref _skipped);
}
