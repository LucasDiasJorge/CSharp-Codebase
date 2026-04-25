namespace AggregationDepartmentManagement.Models;

public sealed class Employee
{
    public Employee(int id, string fullName, string role)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        Id = id;
        FullName = fullName;
        Role = role;
    }

    public int Id { get; }
    public string FullName { get; }
    public string Role { get; }

    public override string ToString() => $"{FullName} ({Role})";
}