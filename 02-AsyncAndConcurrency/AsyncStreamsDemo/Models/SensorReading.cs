namespace AsyncStreamsDemo.Models;

/// <summary>
/// Item produzido pelo stream. <see cref="ProducedAt"/> registra o instante em que a leitura
/// saiu da fonte, para comparar com o instante em que o consumidor a recebeu.
/// </summary>
public sealed record SensorReading(int SequenceNumber, double Value, DateTimeOffset ProducedAt);
