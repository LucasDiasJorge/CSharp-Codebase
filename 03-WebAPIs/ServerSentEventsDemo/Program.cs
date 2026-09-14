using ServerSentEventsDemo.Endpoints;
using ServerSentEventsDemo.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// A mesma instancia serve como fonte de eventos e como servico em segundo plano: o
// gerador precisa ser unico para que os ids de evento sejam globais.
builder.Services.AddSingleton<PriceTickFeed>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<PriceTickFeed>());

WebApplication app = builder.Build();

// Pagina com EventSource para ver a reconexao automatica acontecendo.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapNativeSseEndpoints();
app.MapManualSseEndpoints();

app.Run();
