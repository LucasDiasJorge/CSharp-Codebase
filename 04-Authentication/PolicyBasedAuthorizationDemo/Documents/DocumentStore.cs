namespace PolicyBasedAuthorizationDemo.Documents;

public sealed class DocumentStore
{
    private readonly IReadOnlyList<Document> _documents =
    [
        new Document(1, "Roadmap de engenharia", ownerId: "1", department: "engineering", requiredClearance: 3),
        new Document(2, "Plano de contratacao", ownerId: "2", department: "engineering", requiredClearance: 1),
        new Document(3, "Fechamento trimestral", ownerId: "3", department: "finance", requiredClearance: 5)
    ];

    public IReadOnlyList<Document> GetAll() => _documents;

    public Document? FindById(int id)
    {
        foreach (Document document in _documents)
        {
            if (document.Id == id)
            {
                return document;
            }
        }

        return null;
    }
}
