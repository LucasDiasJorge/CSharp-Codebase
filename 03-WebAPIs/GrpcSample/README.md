# 🔄 gRPC Sample - Server & Client

Exemplo completo de gRPC em .NET 9 cobrindo os 4 tipos de RPC.

---

## 📚 Conceitos Abordados

- **Unary RPC**: Request/Response simples
- **Server Streaming**: Servidor envia múltiplos itens
- **Client Streaming**: Cliente envia stream
- **Bidirectional**: Comunicação bidirecional
- **Protocol Buffers**: Definição de contratos .proto

---

## 🎯 Objetivos de Aprendizado

- Entender geração de stubs a partir de `.proto`
- Implementar cada padrão de streaming
- Separar contratos, servidor e cliente
- Aplicar boas práticas gRPC

---

## 📂 Estrutura do Projeto

```
GrpcSample/
└── src/
    ├── GrpcSample.Contracts/  # Contratos .proto
    ├── GrpcSample.Server/     # Servidor gRPC
    └── GrpcSample.Client/     # Cliente console
```

---

## 🚀 Como Executar

### 1. Servidor

```bash
cd GrpcSample/src/GrpcSample.Server
dotnet run
```

### 2. Cliente (novo terminal)

```bash
cd GrpcSample/src/GrpcSample.Client
dotnet run
```

---

## 📋 Tipos de RPC

| Tipo | Método | Descrição |
|------|--------|-----------|
| **Unary** | `CreateUser` / `GetUser` | Request/Response simples |
| **Server Streaming** | `ListUsers` | Servidor envia múltiplos itens |
| **Client Streaming** | `ImportUsers` | Cliente envia stream, recebe resumo |
| **Bidirectional** | `UserEvents` | Interação contínua bidirecional |

---

## 💡 Exemplo de Contrato (.proto)

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

---

## ✅ Boas Práticas

- Tipos explícitos para clareza
- Repositório isolando persistência
- Health checks para monitoramento
- Reflection para debug (dev only)
- Tratamento de erros com `RpcException`

---

## 🔧 Troubleshooting

| Problema | Solução |
|----------|---------|
| Canal não conecta | Verificar URL/porta do servidor |
| Erro TLS | `dotnet dev-certs https --trust` |
| Porta ocupada | Alterar `launchSettings.json` |

---

## 🔜 Próximos Passos

- Autenticação JWT ou mTLS
- Interceptors para logging/retry
- Persistência real (SQL/NoSQL)
- SDK cliente encapsulado

---

## 🔗 Referências

- [gRPC for .NET](https://docs.microsoft.com/aspnet/core/grpc/)
- [Protocol Buffers](https://developers.google.com/protocol-buffers)
