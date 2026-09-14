using System.Diagnostics;
using ProblemDetailsApi.Handlers;
using ProblemDetailsApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<OrderService>();

builder.Services.AddProblemDetails(options =>
{
    // Aplicado a todo ProblemDetails escrito pelo IProblemDetailsService. E o lugar
    // certo para o que deve aparecer em TODA resposta de erro.
    options.CustomizeProblemDetails = context =>
    {
        // traceId correlaciona a resposta ao log distribuido. E o unico campo que o
        // usuario final precisa repassar ao suporte.
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        context.ProblemDetails.Instance ??= $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
    };
});

// A ordem do registro e a ordem da cadeia: o primeiro handler que devolver true
// encerra o processamento. O de dominio vem antes; o de fallback, por ultimo.
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Sem argumentos, usa a cadeia de IExceptionHandler registrada acima.
app.UseExceptionHandler();

// Da corpo a respostas que sairiam vazias: 404 de rota inexistente, 405 de metodo
// nao permitido. Sem isso o cliente recebe status sem explicacao nenhuma.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
