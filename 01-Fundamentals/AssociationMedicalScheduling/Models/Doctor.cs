namespace AssociationMedicalScheduling.Models;

public sealed class Doctor
{
    public Doctor(int id, string fullName, string specialty)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(specialty);

        Id = id;
        FullName = fullName;
        Specialty = specialty;
    }

    public int Id { get; }
    public string FullName { get; }
    public string Specialty { get; }

    public override string ToString() => $"Dr(a). {FullName} ({Specialty})";
}