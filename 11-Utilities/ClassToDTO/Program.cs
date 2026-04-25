using ClassToDTO.Db;
using Microsoft.EntityFrameworkCore;

namespace ClassToDTO;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql("Host=localhost;Database=csharp_db;Username=postgres;Password=postgres")
            );

        var app = builder.Build();

        app.Run();
    }
}
