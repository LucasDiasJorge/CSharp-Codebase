using Microsoft.EntityFrameworkCore;
using TransactionalOutboxDemo.Messaging;

namespace TransactionalOutboxDemo.Outbox;

/// <summary>
/// O relay: lê as mensagens pendentes e as publica. Roda separado da transação de
/// negócio, e é essa separação que dá a garantia — a transação já commitou, então a
/// mensagem existe e será entregue mais cedo ou mais tarde.
/// </summary>
public sealed class OutboxRelay : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<OutboxRelay> _logger;

    public OutboxRelay(IServiceScopeFactory scopeFactory, RabbitMqPublisher publisher, ILogger<OutboxRelay> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // O relay nunca pode morrer: se ele parar, as mensagens param.
                _logger.LogError(ex, "Ciclo do relay falhou; tentando de novo no proximo intervalo.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        OrdersDbContext database = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        // Ordem de gravação preservada: mensagens de saída publicadas fora de ordem
        // confundem consumidores que dependem de sequência por agregado.
        List<OutboxMessage> pending = await database.OutboxMessages
            .Where(message => message.PublishedAt == null)
            .OrderBy(message => message.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (pending.Count == 0)
        {
            return;
        }

        foreach (OutboxMessage message in pending)
        {
            try
            {
                await _publisher.PublishAsync(message.MessageId, message.Type, message.Payload, cancellationToken)
                    .ConfigureAwait(false);

                message.MarkPublished();
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message);

                _logger.LogWarning(
                    "Falha ao publicar {MensagemId} (tentativa {Tentativa}): {Erro}. A mensagem continua pendente.",
                    message.MessageId,
                    message.AttemptCount,
                    ex.Message);

                // Para no primeiro erro: insistir nas seguintes quebraria a ordem e
                // martelaria um broker que ja esta com problema.
                break;
            }
        }

        await database.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
