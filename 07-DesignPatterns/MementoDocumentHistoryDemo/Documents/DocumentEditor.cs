using System.Text;

namespace MementoDocumentHistoryDemo.Documents;

/// <summary>
/// O originator: o objeto cujo estado se quer salvar e restaurar. Ele é o único que
/// sabe o que compõe o seu estado e o único capaz de ler um memento de volta.
/// </summary>
public sealed class DocumentEditor
{
    private readonly StringBuilder _content = new StringBuilder();
    private List<string> _tags = new List<string>();
    private int _cursor;

    public string Content => _content.ToString();

    public IReadOnlyList<string> Tags => _tags;

    public int Cursor => _cursor;

    public void Type(string text)
    {
        _content.Insert(_cursor, text);
        _cursor += text.Length;
    }

    public void MoveCursor(int position)
    {
        _cursor = Math.Clamp(position, 0, _content.Length);
    }

    public void AddTag(string tag)
    {
        _tags.Add(tag);
    }

    public void Clear()
    {
        _content.Clear();
        _tags.Clear();
        _cursor = 0;
    }

    /// <summary>
    /// Cria o snapshot. A cópia da lista é o ponto: sem <c>new List&lt;string&gt;(_tags)</c>
    /// o memento guardaria uma referência para a MESMA lista que o documento continua
    /// mutando — e restaurar devolveria o estado atual, não o salvo.
    /// </summary>
    public IDocumentMemento Save(string label)
    {
        return new DocumentMemento(label, _content.ToString(), new List<string>(_tags), _cursor);
    }

    /// <summary>
    /// Versão defeituosa, mantida para demonstração: compartilha a referência da lista
    /// de tags em vez de copiá-la. Compila, parece correta e não salva nada.
    /// </summary>
    public IDocumentMemento SaveShallow(string label)
    {
        return new DocumentMemento(label, _content.ToString(), _tags, _cursor);
    }

    /// <summary>
    /// Restaura. Só o originator consegue fazer isto: o cast para o tipo aninhado
    /// privado é possível apenas aqui dentro.
    /// </summary>
    public void Restore(IDocumentMemento memento)
    {
        if (memento is not DocumentMemento snapshot)
        {
            throw new ArgumentException("Memento de outro tipo nao pertence a este editor.", nameof(memento));
        }

        _content.Clear();
        _content.Append(snapshot.Content);
        _tags = new List<string>(snapshot.Tags);
        _cursor = snapshot.Cursor;
    }

    /// <summary>
    /// O memento propriamente dito: classe ANINHADA e PRIVADA. Ninguém fora do
    /// <see cref="DocumentEditor"/> consegue sequer nomear este tipo, quanto mais ler
    /// os seus campos. O histórico manuseia apenas <see cref="IDocumentMemento"/>.
    /// </summary>
    private sealed class DocumentMemento : IDocumentMemento
    {
        public DocumentMemento(string label, string content, List<string> tags, int cursor)
        {
            Label = label;
            Content = content;
            Tags = tags;
            Cursor = cursor;
            CapturedAt = DateTimeOffset.UtcNow;
        }

        public string Label { get; }

        public DateTimeOffset CapturedAt { get; }

        public string Content { get; }

        public List<string> Tags { get; }

        public int Cursor { get; }

        public int ApproximateSizeInChars
        {
            get
            {
                int size = Content.Length;
                foreach (string tag in Tags)
                {
                    size += tag.Length;
                }

                return size;
            }
        }
    }
}
