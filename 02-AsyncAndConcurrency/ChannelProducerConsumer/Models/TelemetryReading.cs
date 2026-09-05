namespace ChannelProducerConsumer.Models;

/// <summary>
/// Item que trafega pelo canal: uma leitura de sensor identificada por um numero
/// de sequencia global, usado para verificar se a ordem foi preservada.
/// </summary>
public sealed record TelemetryReading(int SequenceNumber, string SensorId, double Value);
