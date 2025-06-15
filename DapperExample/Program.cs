using API.Models;
using MySqlConnector;
using Dapper;
using System.Transactions;
using DevOne.Security.Cryptography.BCrypt;
using Microsoft.AspNetCore.Hosting.Server;

namespace DapperExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        DapperTests();

        app.Run();
    }

    public static void DapperTests()
    {
        try
        {
            using (var connection = new MySqlConnection("Server = localhost; Database = my-db; User = root; Password = myrootpass;"))
            {
                connection.Open();

                using (var transaction = new TransactionScope()) // Inicia transação para garantir consistência
                {
                    // Criar uma empresa
                    var insertCompanyQuery = @"INSERT INTO companies (Name) VALUES (@Name);";
                    connection.Execute(insertCompanyQuery, new { Name = "Minha Empresa" });

                    // Obter o ID recém-inserido
                    var companyId = connection.QuerySingle<int>("SELECT LAST_INSERT_ID();");
                    Console.WriteLine($"Empresa inserida com ID: {companyId}");

                    // Criar um usuário vinculado à empresa
                    var insertUserQuery = @"INSERT INTO users (Name, Email, Password, company_id) VALUES (@Name, @Email, @Password, @CompanyId);";

                    var salt = BCryptHelper.GenerateSalt(12); // Gerar um salt para a senha

                    var newUser = new User
                    {
                        Name = "Lucas",
                        Email = "lucas@example.com",
                        Password = BCryptHelper.HashPassword("123456", salt),
                        CompanyId = companyId
                    };

                    var rowsAffected = connection.Execute(insertUserQuery, newUser);
                    Console.WriteLine($"Usuário inserido com sucesso! ID da empresa: {newUser.CompanyId}");

                    // Commit da transação
                    transaction.Complete();
                }

                // Selecionar usuários
                var users = connection.Query<User>("SELECT * FROM users");
                foreach (var user in users)
                {
                    Console.WriteLine($"{user.Id}: {user.Name} - {user.Email}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }


}