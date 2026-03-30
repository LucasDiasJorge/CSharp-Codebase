using AggregationDepartmentManagement.Models;
using AggregationDepartmentManagement.Services;

Console.WriteLine("Aggregation - Gestao de Departamentos");
Console.WriteLine(new string('=', 45));

DepartmentCoordinator coordinator = new DepartmentCoordinator();

coordinator.RegisterEmployee(new Employee(1, "Ana Paula", "Backend Engineer"));
coordinator.RegisterEmployee(new Employee(2, "Diego Sousa", "Data Engineer"));
coordinator.RegisterEmployee(new Employee(3, "Marina Lopes", "QA Analyst"));
coordinator.RegisterEmployee(new Employee(4, "Ricardo Alves", "Product Designer"));

coordinator.CreateDepartment("Plataforma");
coordinator.CreateDepartment("Dados");

coordinator.AssignEmployeeToDepartment(1, "Plataforma");
coordinator.AssignEmployeeToDepartment(2, "Dados");
coordinator.AssignEmployeeToDepartment(3, "Plataforma");

Console.WriteLine("Estado inicial dos departamentos:");
PrintSnapshot(coordinator);

coordinator.TransferEmployee(3, "Dados");
Console.WriteLine();
Console.WriteLine("Marina foi transferida para o departamento de Dados.");
PrintSnapshot(coordinator);

coordinator.DisbandDepartment("Plataforma");
Console.WriteLine();
Console.WriteLine("Departamento Plataforma foi encerrado.");
PrintSnapshot(coordinator);

Console.WriteLine();
Console.WriteLine("Independencia das partes (Aggregation):");
Console.WriteLine("Funcionarios seguem cadastrados mesmo com departamento encerrado.");

static void PrintSnapshot(DepartmentCoordinator coordinator)
{
    IReadOnlyCollection<Department> departments = coordinator.GetDepartments();

    foreach (Department department in departments)
    {
        Console.WriteLine($"- {department.Name}");

        if (department.Members.Count == 0)
        {
            Console.WriteLine("  Sem membros.");
            continue;
        }

        foreach (Employee employee in department.Members)
        {
            Console.WriteLine($"  {employee.FullName} | {employee.Role}");
        }
    }

    IReadOnlyCollection<Employee> unassignedEmployees = coordinator.GetUnassignedEmployees();

    Console.WriteLine("Funcionarios sem departamento:");

    if (unassignedEmployees.Count == 0)
    {
        Console.WriteLine("- Nenhum");
        return;
    }

    foreach (Employee employee in unassignedEmployees)
    {
        Console.WriteLine($"- {employee.FullName} ({employee.Role})");
    }
}