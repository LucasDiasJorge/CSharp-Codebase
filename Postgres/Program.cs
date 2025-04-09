using Postgres.Models;
using System;
using System.Linq;
using Postgres;

class Program
{
    static void Main()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated(); // Cria o banco/tabela se não existir

        if (db.Users.Any())
        {
            db.Users.AddRange(
                new Users { Name = "Alice", Email = "alice@example.com", Password = "1234", Role = Roles.ADMINISTRATOR},
                new Users { Name = "Bob", Email = "bob@example.com", Password = "abcd", Role = Roles.MANAGER },
                new Users { Name = "Carol", Email = "carol@example.com", Password = "4321", Role = Roles.INTERN}
            );
            db.SaveChanges();
        }

        var usersWithA = db.Users
            .Where(u => u.Name.StartsWith("A"))
            .ToList();

        Console.WriteLine("Usuários cujo nome começa com 'A':");
        foreach (var user in usersWithA)
        {
            Console.WriteLine($"{user.Name} - {user.Email} - {user.Role}");
        }
    }
}