# CLAUDE.md — GrpcSample.Server

Servidor gRPC do sample. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/GrpcSample/src/GrpcSample.Server/GrpcSample.Server.csproj
dotnet run --project 03-WebAPIs/GrpcSample/src/GrpcSample.Server/GrpcSample.Server.csproj
```

**Suba o servidor antes do cliente** — o cliente é um console que falha se não houver ninguém escutando.

## Estrutura interna

- `Services/UserGrpcService.cs` — implementa o serviço definido em `GrpcSample.Contracts/Protos/user.proto` (projeto referenciado).
- `Services/GreeterService.cs` — implementa `Protos/greet.proto`, que vive **neste projeto**, não em Contracts.
- `Program.cs` — registra os serviços, mais health checks e server reflection.

Server reflection permite inspecionar a API com grpcurl/Postman sem ter o `.proto` em mãos — é o que torna o sample explorável.

## Pontos de atenção

- TFM `net9.0`. Pacotes: `Grpc.AspNetCore` 2.65.0 + `HealthChecks` + `Server.Reflection`.
- gRPC exige HTTP/2. A porta e o protocolo vêm de `Properties/launchSettings.json`; se o cliente não conectar, confira ali antes de investigar o código.
- **Sem README local** — documentação em `03-WebAPIs/GrpcSample/`.
