
namespace BackgroudWorker;

public class Program
{
    public static void Main(string[] args)
    {
        // Cria builder (config, logging, DI container). Tipagem explícita para reforçar didática.
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        // ------------------------------------------------------------
        // REGISTRO DE SERVIÇOS
        // ------------------------------------------------------------
        // Autorização (mesmo que não haja políticas agora, preserva padrão de pipeline).
        services.AddAuthorization();
        // OpenAPI (descrição de endpoints). Em produção normalmente condicionamos exposição.
        services.AddOpenApi();
        // Hosted Service baseado em timer: executa trabalho periódico em background enquanto o host estiver ativo.
        services.AddHostedService<TimedHostedService>();

        // Constrói a aplicação (após Build, serviços não devem mais ser modificados).
        WebApplication app = builder.Build();

        // ------------------------------------------------------------
        // PIPELINE HTTP
        // ------------------------------------------------------------
        if (app.Environment.IsDevelopment())
        {
            // Exposição de descrição OpenAPI somente em ambiente de desenvolvimento.
            app.MapOpenApi();
        }

        app.UseHttpsRedirection(); // Garante HTTPS (redireciona requisições HTTP).
        app.UseAuthorization();    // Coloca middleware de autorização (mesmo sem políticas custom agora).

        // Endpoint utilitário para sinalizar parada da tarefa (exemplo de interação com hosted service).
        app.MapGet("/stoptask", (HttpContext _) =>
        {
            // IMPORTANTE: StopAsync normalmente é chamado pelo host no shutdown; aqui é demonstrativo.
            TimedHostedService hosted = app.Services.GetRequiredService<TimedHostedService>();
            hosted.StopAsync(new CancellationToken());
        });

        // Inicia a aplicação web + hosted services.
        app.Run();
    }
}
