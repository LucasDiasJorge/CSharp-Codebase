using GoodOrderApi.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar serviços
builder.Services.AddSingleton<OrderRepository>();
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<OrderApplicationService>();
builder.Services.AddSingleton<ReportService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
