using CommandUndoRedoDemo.Editor;

namespace CommandUndoRedoDemo.Commands;

/// <summary>
/// Substituição do documento inteiro. Mostra o custo de memória do padrão: como a
/// operação destrói todo o estado anterior, o comando precisa guardar uma cópia
/// completa dele. Um histórico de cem comandos desses guarda cem documentos.
/// </summary>
public sealed class ReplaceAllCommand : IUndoableCommand
{
    private readonly TextDocument _document;
    private readonly string _newContent;

    private string? _previousContent;

    public ReplaceAllCommand(TextDocument document, string newContent)
    {
        _document = document;
        _newContent = newContent;
    }

    public string Description => $"substituir todo o conteudo por \"{_newContent}\"";

    /// <summary>Tamanho aproximado retido por este comando, em caracteres.</summary>
    public int RetainedCharacters => _previousContent?.Length ?? 0;

    public void Execute()
    {
        _previousContent = _document.Content;
        _document.ReplaceAll(_newContent);
    }

    public void Undo()
    {
        _document.ReplaceAll(_previousContent ?? string.Empty);
    }
}
