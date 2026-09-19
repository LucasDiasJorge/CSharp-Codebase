namespace MementoDocumentHistoryDemo.Documents;

/// <summary>
/// A face pública do memento — deliberadamente quase vazia. O histórico precisa
/// guardar snapshots, rotulá-los e devolvê-los; não precisa (e não deve) conseguir ler
/// ou alterar o estado que eles carregam.
///
/// É essa interface estreita que faz o padrão valer a pena. Expor o estado aqui seria
/// o mesmo que tornar públicos os campos do documento, só que com passos a mais.
/// </summary>
public interface IDocumentMemento
{
    /// <summary>Rótulo para exibir no histórico. Metadado, não estado.</summary>
    string Label { get; }

    DateTimeOffset CapturedAt { get; }

    /// <summary>Tamanho aproximado retido, para tornar o custo do padrão visível.</summary>
    int ApproximateSizeInChars { get; }
}
