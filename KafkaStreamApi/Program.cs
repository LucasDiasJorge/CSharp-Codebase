using KafkaStreamApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar o serviço Kafka com validação
var kafkaConfig = builder.Configuration.GetSection("Kafka");
var bootstrapServers = kafkaConfig["BootstrapServers"] ?? throw new InvalidOperationException("Kafka BootstrapServers not configured");
var groupId = kafkaConfig["GroupId"] ?? throw new InvalidOperationException("Kafka GroupId not configured");

builder.Services.AddSingleton<KafkaConsumerService>(new KafkaConsumerService(
    bootstrapServers,
    groupId
));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSelected", policy =>
    {
        policy.WithOrigins("*")
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Urls.Add("http://0.0.0.0:5000");

app.Run();