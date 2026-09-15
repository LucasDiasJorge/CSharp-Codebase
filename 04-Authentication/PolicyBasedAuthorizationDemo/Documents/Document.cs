namespace PolicyBasedAuthorizationDemo.Documents;

public sealed class Document
{
    public Document(int id, string title, string ownerId, string department, int requiredClearance)
    {
        Id = id;
        Title = title;
        OwnerId = ownerId;
        Department = department;
        RequiredClearance = requiredClearance;
    }

    public int Id { get; }

    public string Title { get; }

    /// <summary>Dono do documento. A autorização por recurso gira em torno deste campo.</summary>
    public string OwnerId { get; }

    public string Department { get; }

    public int RequiredClearance { get; }
}
