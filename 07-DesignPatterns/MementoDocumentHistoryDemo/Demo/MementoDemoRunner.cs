using MementoDocumentHistoryDemo.Documents;
using MementoDocumentHistoryDemo.History;
using Microsoft.Extensions.Logging;

namespace MementoDocumentHistoryDemo.Demo;

/// <summary>
/// Cinco cenários: salvar e restaurar, a opacidade do memento, o bug de cópia rasa,
/// o custo de memória e o limite do histórico.
/// </summary>
public sealed class MementoDemoRunner
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MementoDemoRunner> _logger;

    public MementoDemoRunner(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MementoDemoRunner>();
    }

    public void RunAll()
    {
        RunSaveAndRestore();
        RunEncapsulation();
        RunShallowCopyBug();
        RunMemoryCost();
        RunHistoryLimit();
    }

    private void RunSaveAndRestore()
    {
        Section("1. Salvar e restaurar o estado");

        DocumentEditor editor = new DocumentEditor();
        DocumentHistory history = new DocumentHistory(_loggerFactory.CreateLogger<DocumentHistory>());

        editor.Type("Relatorio trimestral");
        editor.AddTag("rascunho");
        history.Push(editor.Save("v1"));
        Show(editor, "apos v1");

        editor.Type(" - revisado");
        editor.AddTag("revisao");
        editor.MoveCursor(0);
        Show(editor, "apos edicoes");

        IDocumentMemento? v1 = history.Find("v1");
        editor.Restore(v1!);
        Show(editor, "apos restaurar v1");

        _logger.LogInformation("Conteudo, tags e cursor voltaram todos ao ponto salvo.");
    }

    private void RunEncapsulation()
    {
        Section("2. O que o caretaker consegue ver de um snapshot");

        DocumentEditor editor = new DocumentEditor();
        DocumentHistory history = new DocumentHistory(_loggerFactory.CreateLogger<DocumentHistory>());

        editor.Type("Conteudo confidencial");
        editor.AddTag("sigiloso");
        history.Push(editor.Save("confidencial"));

        foreach (string description in history.Describe())
        {
            _logger.LogInformation("  historico ve: {Descricao}", description);
        }

        _logger.LogInformation(
            "So rotulo, tamanho e horario. O conteudo nao aparece porque IDocumentMemento nao o expoe — " +
            "e o tipo concreto e uma classe aninhada privada do editor, que ninguem de fora consegue nomear.");
    }

    private void RunShallowCopyBug()
    {
        Section("3. Copia rasa: o snapshot que nao salva nada");

        DocumentEditor broken = new DocumentEditor();
        broken.Type("Texto");
        broken.AddTag("original");

        // Snapshot defeituoso: compartilha a referencia da lista de tags.
        IDocumentMemento shallow = broken.SaveShallow("raso");

        broken.AddTag("adicionada-depois");
        broken.Restore(shallow);

        _logger.LogWarning(
            "Com copia rasa, apos restaurar as tags sao [{Tags}] — a tag adicionada DEPOIS do snapshot sobreviveu.",
            string.Join(", ", broken.Tags));

        DocumentEditor correct = new DocumentEditor();
        correct.Type("Texto");
        correct.AddTag("original");

        IDocumentMemento deep = correct.Save("profundo");

        correct.AddTag("adicionada-depois");
        correct.Restore(deep);

        _logger.LogInformation(
            "Com copia profunda, apos restaurar as tags sao [{Tags}] — o estado salvo foi preservado.",
            string.Join(", ", correct.Tags));
    }

    private void RunMemoryCost()
    {
        Section("4. Custo: cada snapshot e uma copia completa");

        DocumentEditor editor = new DocumentEditor();
        DocumentHistory history = new DocumentHistory(_loggerFactory.CreateLogger<DocumentHistory>(), limit: 10);

        editor.Type(new string('x', 500));

        for (int index = 1; index <= 4; index++)
        {
            editor.Type(new string('y', 100));
            history.Push(editor.Save($"snap-{index}"));
        }

        _logger.LogInformation(
            "Documento com {Tamanho} chars e 4 snapshots retem {Total} chars no historico — " +
            "cada ponto de restauracao guarda o documento inteiro, nao a diferenca.",
            editor.Content.Length,
            history.TotalRetainedChars);
    }

    private void RunHistoryLimit()
    {
        Section("5. Limite do historico");

        DocumentEditor editor = new DocumentEditor();
        DocumentHistory history = new DocumentHistory(_loggerFactory.CreateLogger<DocumentHistory>(), limit: 3);

        for (int index = 1; index <= 5; index++)
        {
            editor.Type($"linha {index}. ");
            history.Push(editor.Save($"v{index}"));
        }

        _logger.LogInformation("Snapshots disponiveis: {Lista}", string.Join(" | ", history.Describe()));
        _logger.LogInformation("As duas primeiras versoes nao podem mais ser restauradas.");
    }

    private void Show(DocumentEditor editor, string moment)
    {
        _logger.LogInformation(
            "  {Momento}: \"{Conteudo}\" | tags=[{Tags}] | cursor={Cursor}",
            moment,
            editor.Content,
            string.Join(", ", editor.Tags),
            editor.Cursor);
    }

    private void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }
}
