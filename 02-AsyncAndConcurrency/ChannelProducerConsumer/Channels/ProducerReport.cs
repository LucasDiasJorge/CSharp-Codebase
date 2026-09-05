namespace ChannelProducerConsumer.Channels;

/// <summary>
/// O que um produtor observou durante a escrita: quantos itens entregou ao canal,
/// quanto tempo ficou parado esperando vaga e a maior fila que viu.
/// </summary>
public sealed record ProducerReport(int WrittenCount, TimeSpan BlockedTime, int MaxQueueDepth);
