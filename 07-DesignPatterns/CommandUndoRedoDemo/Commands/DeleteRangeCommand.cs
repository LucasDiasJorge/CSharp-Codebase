using CommandUndoRedoDemo.Editor;

namespace CommandUndoRedoDemo.Commands;

/// <summary>
/// Remoção. Aqui aparece a exigência real do padrão: para desfazer é preciso ter
/// guardado o que foi apagado. O texto removido só existe no instante da execução —
/// se o comando não o capturar, a informação some e o undo vira impossível.
/// </summary>
public sealed class DeleteRangeCommand : IUndoableCommand
{
    private readonly TextDocument _document;
    private readonly int _position;
    private readonly int _length;

    private string? _removedText;

    public DeleteRangeCommand(TextDocument document, int position, int length)
    {
        _document = document;
        _position = position;
        _length = length;
    }

    public string Description => $"remover {_length} caracteres da posicao {_position}";

    public void Execute()
    {
        _removedText = _document.Remove(_position, _length);
    }

    public void Undo()
    {
        if (_removedText is null)
        {
            throw new InvalidOperationException("Undo chamado antes de Execute.");
        }

        _document.Insert(_position, _removedText);
    }
}
