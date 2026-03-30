namespace AssociationMedicalScheduling.Models;

public sealed class Patient
{
    public Patient(int id, string fullName, DateOnly birthDate)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        Id = id;
        FullName = fullName;
        BirthDate = birthDate;
    }

    public int Id { get; }
    public string FullName { get; }
    public DateOnly BirthDate { get; }

    public override string ToString() => $"{FullName} (Id={Id})";
}