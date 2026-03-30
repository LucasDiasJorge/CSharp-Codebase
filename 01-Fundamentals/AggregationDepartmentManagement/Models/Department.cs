namespace AggregationDepartmentManagement.Models;

public sealed class Department
{
    private readonly List<Employee> members;

    public Department(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        members = new List<Employee>();
    }

    public string Name { get; }
    public IReadOnlyCollection<Employee> Members => members.AsReadOnly();

    internal void AddMember(Employee employee)
    {
        if (!members.Contains(employee))
        {
            members.Add(employee);
        }
    }

    internal void RemoveMember(Employee employee)
    {
        if (members.Contains(employee))
        {
            members.Remove(employee);
        }
    }

    internal IReadOnlyCollection<Employee> ReleaseAllMembers()
    {
        List<Employee> releasedMembers = new List<Employee>(members);
        members.Clear();

        return releasedMembers;
    }
}