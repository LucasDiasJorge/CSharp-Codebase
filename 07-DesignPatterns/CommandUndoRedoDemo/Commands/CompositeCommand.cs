namespace CommandUndoRedoDemo.Commands;

/// <summary>
/// Vários comandos tratados como um só: o usuário desfaz a operação inteira de uma vez,
/// não passo a passo. O detalhe que importa é a ordem — desfazer é na ordem INVERSA da
/// execução, porque cada comando pressupõe o estado deixado pelo anterior.
/// </summary>
public sealed class CompositeCommand : IUndoableCommand
{
    private readonly IReadOnlyList<IUndoableCommand> _commands;

    public CompositeCommand(string description, IReadOnlyList<IUndoableCommand> commands)
    {
        Description = description;
        _commands = commands;
    }

    public string Description { get; }

    public void Execute()
    {
        foreach (IUndoableCommand command in _commands)
        {
            command.Execute();
        }
    }

    public void Undo()
    {
        for (int index = _commands.Count - 1; index >= 0; index--)
        {
            _commands[index].Undo();
        }
    }
}
