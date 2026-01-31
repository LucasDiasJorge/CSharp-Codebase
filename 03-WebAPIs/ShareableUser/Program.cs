namespace ShareableUser;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Registra o serviço singleton
        builder.Services.AddSingleton<IUsuarioSingletonService, UsuarioSingletonService>();

        // Configura logging para ver melhor os resultados
        builder.Services.AddLogging(configure => 
            configure.AddConsole().SetMinimumLevel(LogLevel.Information));

        var app = builder.Build();

        // Adiciona o middleware
        app.UseMiddleware<UsuarioMonitorMiddleware>();

        // Rota simples para testar
        app.MapGet("/", (IUsuarioSingletonService usuario) => 
        {
            return Results.Ok(new {
                Usuario = usuario.NomeUsuario,
                InstanceId = usuario.InstanceId,
                Acessos = usuario.ContadorAcessos
            });
        });

        app.Run();
    }
}
