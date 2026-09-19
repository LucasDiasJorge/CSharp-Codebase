using MementoDocumentHistoryDemo.Documents;
using Microsoft.Extensions.Logging;

namespace MementoDocumentHistoryDemo.History;

/// <summary>
/// O caretaker: guarda os snapshots e nada mais. Repare no que ele **não** consegue
/// fazer — ler o conteúdo salvo, alterá-lo, ou construir um memento do zero. Ele
/// manuseia apenas <see cref="IDocumentMemento"/>, que não expõe estado nenhum.
/// </summary>
public sealed class DocumentHistory
{
    private readonly List<IDocumentMemento> _snapshots = new List<IDocumentMemento>();
    private readonly int _limit;
    private readonly ILogger<DocumentHistory> _logger;

    public DocumentHistory(ILogger<DocumentHistory> logger, int limit = 5)
    {
        _logger = logger;
        _limit = limit;
    }

    public int Count => _snapshots.Count;

    /// <summary>Soma do que todos os snapshots retêm — o custo do padrão, em número.</summary>
    public int TotalRetainedChars
    {
        get
        {
            int total = 0;
            foreach (IDocumentMemento snapshot in _snapshots)
            {
                total += snapshot.ApproximateSizeInChars;
            }

            return total;
        }
    }

    public void Push(IDocumentMemento memento)
    {
        _snapshots.Add(memento);

        if (_snapshots.Count > _limit)
        {
            // Snapshot completo por ponto de restauracao: sem limite, o historico cresce
            // proporcionalmente ao tamanho do documento vezes o numero de salvamentos.
            IDocumentMemento discarded = _snapshots[0];
            _snapshots.RemoveAt(0);

            _logger.LogInformation(
                "Limite de {Limite} atingido: snapshot \"{Rotulo}\" descartado ({Tamanho} chars liberados).",
                _limit,
                discarded.Label,
                discarded.ApproximateSizeInChars);
        }

        _logger.LogInformation(
            "Snapshot \"{Rotulo}\" guardado. Total: {Total} snapshots, {Chars} chars retidos.",
            memento.Label,
            _snapshots.Count,
            TotalRetainedChars);
    }

    public IDocumentMemento? Pop()
    {
        if (_snapshots.Count == 0)
        {
            return null;
        }

        IDocumentMemento last = _snapshots[^1];
        _snapshots.RemoveAt(_snapshots.Count - 1);

        return last;
    }

    public IDocumentMemento? Find(string label)
    {
        foreach (IDocumentMemento snapshot in _snapshots)
        {
            if (snapshot.Label == label)
            {
                return snapshot;
            }
        }

        return null;
    }

    public IReadOnlyList<string> Describe()
    {
        List<string> descriptions = new List<string>();
        foreach (IDocumentMemento snapshot in _snapshots)
        {
            // Tudo o que o caretaker pode dizer sobre um snapshot: rotulo, quando e
            // quanto ocupa. O conteudo permanece inacessivel.
            descriptions.Add($"{snapshot.Label} ({snapshot.ApproximateSizeInChars} chars, {snapshot.CapturedAt:HH:mm:ss})");
        }

        return descriptions;
    }
}
