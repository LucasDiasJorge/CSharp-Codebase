using CommandUndoRedoDemo.Commands;
using Microsoft.Extensions.Logging;

namespace CommandUndoRedoDemo.Editor;

/// <summary>
/// O invoker: executa comandos e mantém o histórico. Duas pilhas bastam — o que já foi
/// feito e o que foi desfeito. É aqui que o undo/redo existe; nem o documento nem os
/// comandos sabem que há histórico.
/// </summary>
public sealed class CommandHistory
{
    private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
    private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();
    private readonly int _limit;
    private readonly ILogger<CommandHistory> _logger;

    public CommandHistory(ILogger<CommandHistory> logger, int limit = 10)
    {
        _logger = logger;
        _limit = limit;
    }

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    public int UndoCount => _undoStack.Count;

    public int RedoCount => _redoStack.Count;

    public void Execute(IUndoableCommand command)
    {
        command.Execute();
        _undoStack.Push(command);

        // Executar algo novo depois de um undo descarta o que havia sido desfeito. O
        // histórico é uma linha, não uma árvore: manter o redo aqui permitiria
        // "refazer" uma operação que pressupunha um estado que não existe mais.
        if (_redoStack.Count > 0)
        {
            _logger.LogInformation("Novo comando executado: {Quantidade} operacoes de redo descartadas.", _redoStack.Count);
            _redoStack.Clear();
        }

        TrimToLimit();

        _logger.LogInformation("Executado: {Descricao}", command.Description);
    }

    public bool Undo()
    {
        if (!CanUndo)
        {
            return false;
        }

        IUndoableCommand command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);

        _logger.LogInformation("Desfeito: {Descricao}", command.Description);

        return true;
    }

    public bool Redo()
    {
        if (!CanRedo)
        {
            return false;
        }

        IUndoableCommand command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);

        _logger.LogInformation("Refeito: {Descricao}", command.Description);

        return true;
    }

    public IReadOnlyList<string> DescribeUndoStack()
    {
        List<string> descriptions = new List<string>();
        foreach (IUndoableCommand command in _undoStack)
        {
            descriptions.Add(command.Description);
        }

        return descriptions;
    }

    /// <summary>
    /// Histórico ilimitado é vazamento de memória com outro nome: cada comando retém o
    /// estado necessário para reverter. Descartar o mais antigo custa a possibilidade
    /// de desfazer até lá — é a troca que todo editor faz.
    /// </summary>
    private void TrimToLimit()
    {
        if (_undoStack.Count <= _limit)
        {
            return;
        }

        IUndoableCommand[] kept = _undoStack.Take(_limit).ToArray();
        _undoStack.Clear();

        for (int index = kept.Length - 1; index >= 0; index--)
        {
            _undoStack.Push(kept[index]);
        }

        _logger.LogInformation("Historico no limite de {Limite}: a operacao mais antiga saiu.", _limit);
    }
}
