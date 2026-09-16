using System.Collections.Concurrent;

namespace DeadLetterQueueDemo.Processing;

/// <summary>
/// Histórico do que o consumidor tentou. Existe só para o exemplo: torna visível a
/// sequência de tentativas de cada mensagem, que de outra forma só apareceria no log.
/// </summary>
public sealed class ProcessingLog
{
    private readonly ConcurrentQueue<ProcessingAttempt> _attempts = new ConcurrentQueue<ProcessingAttempt>();

    public void Record(string payload, int attempt, ProcessingResult result)
    {
        _attempts.Enqueue(new ProcessingAttempt(
            DateTimeOffset.UtcNow,
            payload,
            attempt,
            result.Outcome.ToString(),
            result.Detail));
    }

    public IReadOnlyList<ProcessingAttempt> GetAll() => _attempts.ToArray();

    public void Clear()
    {
        while (_attempts.TryDequeue(out _))
        {
        }
    }
}

public sealed record ProcessingAttempt(
    DateTimeOffset At,
    string Payload,
    int Attempt,
    string Outcome,
    string Detail);
