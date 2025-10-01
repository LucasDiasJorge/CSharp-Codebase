using CustomFilterApi.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços
builder.Services.AddControllers();

// Registra o filtro customizado como Scoped para poder usar injeção de dependência
// Isso é necessário porque o filtro usa ILogger injetado
builder.Services.AddScoped<LogPropertyFilter>();

// Registra as implementações de serviços de negócio e o accessor que será usado
// para armazenar a instância selecionada por requisição.
builder.Services.AddScoped<CustomFilterApi.Services.BusinessServiceA>();
builder.Services.AddScoped<CustomFilterApi.Services.BusinessServiceB>();
builder.Services.AddScoped<CustomFilterApi.Services.ISelectedServiceAccessor, CustomFilterApi.Services.SelectedServiceAccessor>();

// Configuração do Swagger/OpenAPI para documentação da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Custom Filter API",
        Version = "v1",
        Description = "API demonstrando o uso de filtros customizados no ASP.NET Core para logging de propriedades específicas"
    });
});

var app = builder.Build();

// Pipeline de middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Importante: MapControllers habilita o roteamento para os controllers
app.MapControllers();

app.Run();
