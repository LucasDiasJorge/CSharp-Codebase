using Grpc.HealthCheck;
using Grpc.Health.V1;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using GrpcSample.Contracts.Users.V1;
using GrpcSample.Server.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddGrpcHealthChecks();
builder.Services.AddGrpcReflection();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

WebApplication app = builder.Build();

app.MapGrpcService<UserGrpcService>();
app.MapGrpcHealthChecksService();
app.MapGrpcReflectionService();

app.MapGet("/", () => "This is a gRPC server. Use a gRPC client.");

app.Run();
