using AggregationDepartmentManagement.Models;

namespace AggregationDepartmentManagement.Services;

public sealed class DepartmentCoordinator
{
    private readonly List<Employee> employees;
    private readonly List<Department> departments;
    private readonly Dictionary<int, Department> allocationByEmployeeId;

    public DepartmentCoordinator()
    {
        employees = new List<Employee>();
        departments = new List<Department>();
        allocationByEmployeeId = new Dictionary<int, Department>();
    }

    public void RegisterEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        Employee? existingEmployee = FindEmployeeById(employee.Id);

        if (existingEmployee is not null)
        {
            throw new InvalidOperationException($"Employee with id {employee.Id} already exists.");
        }

        employees.Add(employee);
    }

    public Department CreateDepartment(string departmentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(departmentName);

        Department? existingDepartment = FindDepartmentByName(departmentName);

        if (existingDepartment is not null)
        {
            throw new InvalidOperationException($"Department {departmentName} already exists.");
        }

        Department department = new Department(departmentName);
        departments.Add(department);

        return department;
    }

    public void AssignEmployeeToDepartment(int employeeId, string departmentName)
    {
        Employee employee = FindEmployeeById(employeeId)
            ?? throw new InvalidOperationException($"Employee with id {employeeId} was not found.");

        Department targetDepartment = FindDepartmentByName(departmentName)
            ?? throw new InvalidOperationException($"Department {departmentName} was not found.");

        if (allocationByEmployeeId.TryGetValue(employeeId, out Department? currentDepartment))
        {
            if (ReferenceEquals(currentDepartment, targetDepartment))
            {
                return;
            }

            currentDepartment.RemoveMember(employee);
        }

        targetDepartment.AddMember(employee);
        allocationByEmployeeId[employeeId] = targetDepartment;
    }

    public void TransferEmployee(int employeeId, string targetDepartmentName)
    {
        AssignEmployeeToDepartment(employeeId, targetDepartmentName);
    }

    public void DisbandDepartment(string departmentName)
    {
        Department department = FindDepartmentByName(departmentName)
            ?? throw new InvalidOperationException($"Department {departmentName} was not found.");

        IReadOnlyCollection<Employee> releasedMembers = department.ReleaseAllMembers();

        foreach (Employee employee in releasedMembers)
        {
            allocationByEmployeeId.Remove(employee.Id);
        }

        departments.Remove(department);
    }

    public IReadOnlyCollection<Department> GetDepartments()
    {
        return departments.AsReadOnly();
    }

    public IReadOnlyCollection<Employee> GetUnassignedEmployees()
    {
        List<Employee> unassignedEmployees = new List<Employee>();

        foreach (Employee employee in employees)
        {
            if (!allocationByEmployeeId.ContainsKey(employee.Id))
            {
                unassignedEmployees.Add(employee);
            }
        }

        return unassignedEmployees;
    }

    private Employee? FindEmployeeById(int employeeId)
    {
        foreach (Employee employee in employees)
        {
            if (employee.Id == employeeId)
            {
                return employee;
            }
        }

        return null;
    }

    private Department? FindDepartmentByName(string departmentName)
    {
        foreach (Department department in departments)
        {
            bool sameName = department.Name.Equals(departmentName, StringComparison.OrdinalIgnoreCase);

            if (sameName)
            {
                return department;
            }
        }

        return null;
    }
}