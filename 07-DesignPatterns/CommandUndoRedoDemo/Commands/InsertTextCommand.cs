using CommandUndoRedoDemo.Editor;

namespace CommandUndoRedoDemo.Commands;

/// <summary>
/// Inserção. O caso fácil: para desfazer basta remover o que foi inserido, e as duas
/// informações necessárias (posição e tamanho) já estão nos parâmetros.
/// </summary>
public sealed class InsertTextCommand : IUndoableCommand
{
    private readonly TextDocument _document;
    private readonly int _position;
    private readonly string _text;

    public InsertTextCommand(TextDocument document, int position, string text)
    {
        _document = document;
        _position = position;
        _text = text;
    }

    public string Description => $"inserir \"{_text}\" na posicao {_position}";

    public void Execute()
    {
        _document.Insert(_position, _text);
    }

    public void Undo()
    {
        _document.Remove(_position, _text.Length);
    }
}
