using CommandUndoRedoDemo.Commands;
using CommandUndoRedoDemo.Editor;
using Microsoft.Extensions.Logging;

namespace CommandUndoRedoDemo.Demo;

/// <summary>
/// Roteiro em cinco cenários, do caso simples às duas armadilhas: o redo descartado por
/// um comando novo e o limite do histórico.
/// </summary>
public sealed class EditorDemoRunner
{
    private readonly TextDocument _document;
    private readonly CommandHistory _history;
    private readonly ILogger<EditorDemoRunner> _logger;

    public EditorDemoRunner(TextDocument document, CommandHistory history, ILogger<EditorDemoRunner> logger)
    {
        _document = document;
        _history = history;
        _logger = logger;
    }

    public void RunAll()
    {
        RunBasicUndoRedo();
        RunDeleteAndRestore();
        RunCompositeCommand();
        RunRedoDiscarded();
        RunHistoryLimit();
    }

    private void RunBasicUndoRedo()
    {
        Section("1. Executar, desfazer e refazer");

        _history.Execute(new InsertTextCommand(_document, 0, "Ola"));
        _history.Execute(new InsertTextCommand(_document, 3, ", mundo"));
        ShowDocument();

        _history.Undo();
        ShowDocument();

        _history.Redo();
        ShowDocument();
    }

    private void RunDeleteAndRestore()
    {
        Section("2. Remocao: o comando precisa guardar o que apagou");

        _history.Execute(new DeleteRangeCommand(_document, 0, 5));
        ShowDocument();

        _history.Undo();
        ShowDocument();

        _logger.LogInformation("O texto voltou porque o comando havia capturado o trecho removido.");
    }

    private void RunCompositeCommand()
    {
        Section("3. Comando composto: varios passos, um unico undo");

        CompositeCommand composite = new CompositeCommand(
            "formatar como titulo",
            [
                new DeleteRangeCommand(_document, 0, _document.Length),
                new InsertTextCommand(_document, 0, "# OLA, MUNDO"),
                new InsertTextCommand(_document, 12, " #")
            ]);

        _history.Execute(composite);
        ShowDocument();

        _history.Undo();
        ShowDocument();

        _logger.LogInformation("Os tres passos foram revertidos juntos, na ordem inversa da execucao.");
    }

    private void RunRedoDiscarded()
    {
        Section("4. Executar algo novo apos um undo descarta o redo");

        _history.Execute(new InsertTextCommand(_document, _document.Length, " [rascunho]"));
        ShowDocument();

        _history.Undo();
        _logger.LogInformation("Apos o undo: {Redo} operacao(oes) disponivel(is) para refazer.", _history.RedoCount);

        _history.Execute(new InsertTextCommand(_document, _document.Length, " [final]"));
        _logger.LogInformation("Apos o novo comando: {Redo} operacao(oes) para refazer.", _history.RedoCount);

        bool redone = _history.Redo();
        _logger.LogInformation("Tentativa de redo: {Resultado}.", redone ? "funcionou" : "nao ha o que refazer");
        ShowDocument();
    }

    private void RunHistoryLimit()
    {
        Section("5. Limite do historico");

        for (int index = 1; index <= 12; index++)
        {
            _history.Execute(new InsertTextCommand(_document, _document.Length, $" #{index}"));
        }

        _logger.LogInformation(
            "Depois de 12 comandos, o historico guarda {Undo} (limite configurado) — os mais antigos nao podem mais ser desfeitos.",
            _history.UndoCount);
    }

    private void ShowDocument()
    {
        _logger.LogInformation("Documento: \"{Conteudo}\" | undo={Undo} redo={Redo}", _document.Content, _history.UndoCount, _history.RedoCount);
    }

    private void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
