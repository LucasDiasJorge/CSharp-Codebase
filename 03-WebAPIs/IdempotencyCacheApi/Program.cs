using IdempotencyCacheApi.Models;
using IdempotencyCacheApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.Configure<IdempotencyCacheOptions>(
    builder.Configuration.GetSection(IdempotencyCacheOptions.SectionName));
builder.Services.AddSingleton<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<IPaymentProcessor, PaymentProcessor>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
