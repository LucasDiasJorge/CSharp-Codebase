using Grpc.HealthCheck;
using Grpc.Health.V1;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using GrpcSample.Contracts.Users.V1;
using GrpcSample.Server.Services;

// ------------------------------------------------------------
// gRPC SERVER BOOTSTRAP
// ------------------------------------------------------------
// Este Program.cs demonstra registro explícito de serviços gRPC com health checks e reflection
// (reflection é útil em desenvolvimento para ferramentas como grpcurl / bloomrpc; evite em prod sem controle de acesso).

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Registro principal do runtime gRPC.
builder.Services.AddGrpc();
// Health checks específicos para gRPC (expõe serviço padrão do pacote Grpc.HealthCheck).
builder.Services.AddGrpcHealthChecks();
// Reflection service para inspeção dinâmica de contratos/métodos.
builder.Services.AddGrpcReflection();
// Repositório em memória (simples) — facilmente substituível por implementação persistente futura.
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

WebApplication app = builder.Build();

// ------------------------------------------------------------
// MAPEAMENTO DE ENDPOINTS gRPC
// ------------------------------------------------------------
app.MapGrpcService<UserGrpcService>();            // Serviço principal (Unary + Streaming)
app.MapGrpcHealthChecksService();                 // /grpc.health.v1.Health/Check
app.MapGrpcReflectionService();                   // Reflection para ferramentas de debug

// Endpoint HTTP simples para orientações quando alguém acessa via navegador.
app.MapGet("/", () => "This is a gRPC server. Use a gRPC client.");

// Inicia o host (HTTP/2) - gRPC requer HTTP/2 (Kestrel já configurado automaticamente em dev para HTTPS).
app.Run();
