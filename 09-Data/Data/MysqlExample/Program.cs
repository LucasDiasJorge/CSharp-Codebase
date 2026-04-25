using Microsoft.EntityFrameworkCore;

namespace MysqlExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<MeuDbContext>(options =>
            options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();
        app.Run();
    }
}
