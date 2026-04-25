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
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        WebApplication app = builder.Build();

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
            using (MySqlConnection connection = new MySqlConnection("Server = localhost; Database = my-db; User = root; Password = myrootpass;"))
            {
                connection.Open();

                using (TransactionScope transaction = new TransactionScope()) // Inicia transa��o para garantir consist�ncia
                {
                    // Criar uma empresa
                    string insertCompanyQuery = @"INSERT INTO companies (Name) VALUES (@Name);";
                    connection.Execute(insertCompanyQuery, new { Name = "Minha Empresa" });

                    // Obter o ID rec�m-inserido
                    int companyId = connection.QuerySingle<int>("SELECT LAST_INSERT_ID();");
                    Console.WriteLine($"Empresa inserida com ID: {companyId}");

                    // Criar um usu�rio vinculado � empresa
                    string insertUserQuery = @"INSERT INTO users (Name, Email, Password, company_id) VALUES (@Name, @Email, @Password, @CompanyId);";

                    string salt = BCryptHelper.GenerateSalt(12); // Gerar um salt para a senha

                    User newUser = new User
                    {
                        Name = "Lucas",
                        Email = "lucas@example.com",
                        Password = BCryptHelper.HashPassword("123456", salt),
                        CompanyId = companyId
                    };
                    int rowsAffected = connection.Execute(insertUserQuery, newUser);
                    Console.WriteLine($"Usu�rio inserido com sucesso! ID da empresa: {newUser.CompanyId}");

                    // Commit da transa��o
                    transaction.Complete();
                }

                // Selecionar usu�rios
                System.Collections.Generic.IEnumerable<User> users = connection.Query<User>("SELECT * FROM users");
                foreach (User user in users)
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