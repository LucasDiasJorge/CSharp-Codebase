namespace CancellationTokenPipeline.Models;

/// <summary>
/// Item que atravessa as etapas do pipeline.
/// </summary>
public sealed class DocumentItem
{
    public DocumentItem(int identifier, string rawContent)
    {
        Identifier = identifier;
        RawContent = rawContent;
    }

    public int Identifier { get; }

    public string RawContent { get; }

    public string? TransformedContent { get; private set; }

    public bool IsPublished { get; private set; }

    public void ApplyTransformation(string transformedContent)
    {
        TransformedContent = transformedContent;
    }

    public void MarkAsPublished()
    {
        IsPublished = true;
    }
}
