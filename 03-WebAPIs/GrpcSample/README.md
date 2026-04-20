# gRPC Sample - Server & Client

## Visão geral

Exemplo completo de gRPC em .NET 9 cobrindo os 4 tipos de RPC.

## Conceitos abordados

- **Unary RPC**: Request/Response simples
- **Server Streaming**: Servidor envia múltiplos itens
- **Client Streaming**: Cliente envia stream
- **Bidirectional**: Comunicação bidirecional
- **Protocol Buffers**: Definição de contratos .proto

## Objetivos de aprendizagem

- Entender geração de stubs a partir de `.proto`
- Implementar cada padrão de streaming
- Separar contratos, servidor e cliente
- Aplicar boas práticas gRPC

## Estrutura do projeto

```text
GrpcSample/
+-- src/
|   +-- GrpcSample.Client/
|   +-- GrpcSample.Contracts/
|   \-- GrpcSample.Server/
\-- GrpcSample.sln
```

## Como executar

Escolha um dos projetos abaixo para execução direcionada:

- `dotnet run --project 03-WebAPIs/GrpcSample/src/GrpcSample.Client/GrpcSample.Client.csproj`
- `dotnet build 03-WebAPIs/GrpcSample/src/GrpcSample.Contracts/GrpcSample.Contracts.csproj`
- `dotnet run --project 03-WebAPIs/GrpcSample/src/GrpcSample.Server/GrpcSample.Server.csproj`

## Boas práticas e pontos de atenção

- Tipos explícitos para clareza
- Repositório isolando persistência
- Health checks para monitoramento
- Reflection para debug (dev only)
- Tratamento de erros com `RpcException`

## Conteúdo complementar

##### Estrutura do Projeto

```
GrpcSample/
└── src/
    ├── GrpcSample.Contracts/  # Contratos .proto
    ├── GrpcSample.Server/     # Servidor gRPC
    └── GrpcSample.Client/     # Cliente console
```

##### 1. Servidor

```bash
cd GrpcSample/src/GrpcSample.Server
dotnet run
```

##### 2. Cliente (novo terminal)

```bash
cd GrpcSample/src/GrpcSample.Client
dotnet run
```

##### Tipos de RPC

| Tipo | Método | Descrição |
|------|--------|-----------|
| **Unary** | `CreateUser` / `GetUser` | Request/Response simples |
| **Server Streaming** | `ListUsers` | Servidor envia múltiplos itens |
| **Client Streaming** | `ImportUsers` | Cliente envia stream, recebe resumo |
| **Bidirectional** | `UserEvents` | Interação contínua bidirecional |

##### Exemplo de Contrato (.proto)

```protobuf
service UserService {
  rpc CreateUser (CreateUserRequest) returns (UserResponse);
  rpc GetUser (GetUserRequest) returns (UserResponse);
  rpc ListUsers (ListUsersRequest) returns (stream UserResponse);
  rpc ImportUsers (stream CreateUserRequest) returns (ImportSummary);
}

message User {
  string id = 1;
  string name = 2;
  string email = 3;
}
```

##### Troubleshooting

| Problema | Solução |
|----------|---------|
| Canal não conecta | Verificar URL/porta do servidor |
| Erro TLS | `dotnet dev-certs https --trust` |
| Porta ocupada | Alterar `launchSettings.json` |

##### Próximos Passos

- Autenticação JWT ou mTLS
- Interceptors para logging/retry
- Persistência real (SQL/NoSQL)
- SDK cliente encapsulado

## Referências

- [gRPC for .NET](https://docs.microsoft.com/aspnet/core/grpc/)
- [Protocol Buffers](https://developers.google.com/protocol-buffers)
