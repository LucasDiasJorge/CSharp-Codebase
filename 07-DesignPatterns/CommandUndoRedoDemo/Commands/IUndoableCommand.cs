namespace CommandUndoRedoDemo.Commands;

/// <summary>
/// O contrato do padrão. A operação deixa de ser uma chamada de método e passa a ser um
/// objeto: dá para guardar em lista, enfileirar, repetir e — por causa de
/// <see cref="Undo"/> — reverter.
/// </summary>
public interface IUndoableCommand
{
    /// <summary>Descrição legível, para exibir no histórico.</summary>
    string Description { get; }

    void Execute();

    /// <summary>
    /// Desfaz o efeito de <see cref="Execute"/>. Só é possível porque o comando guarda
    /// o que precisa para reverter — e é essa captura, não o método em si, que dá
    /// trabalho.
    /// </summary>
    void Undo();
}
