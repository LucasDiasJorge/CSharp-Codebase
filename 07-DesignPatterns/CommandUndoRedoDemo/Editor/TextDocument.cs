using System.Text;

namespace CommandUndoRedoDemo.Editor;

/// <summary>
/// O receptor do padrão Command: quem sabe fazer o trabalho de fato. Repare que ele
/// não conhece comando, histórico nem undo — expõe apenas operações primitivas. Toda a
/// capacidade de desfazer vive nos comandos, não aqui.
/// </summary>
public sealed class TextDocument
{
    private readonly StringBuilder _content = new StringBuilder();

    public string Content => _content.ToString();

    public int Length => _content.Length;

    public void Insert(int position, string text)
    {
        _content.Insert(position, text);
    }

    public string Remove(int position, int length)
    {
        string removed = _content.ToString(position, length);
        _content.Remove(position, length);

        return removed;
    }

    public void ReplaceAll(string text)
    {
        _content.Clear();
        _content.Append(text);
    }
}
