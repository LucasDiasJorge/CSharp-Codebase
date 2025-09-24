# gRPC em C# — Exemplo Completo (Server + Client)

Este projeto demonstra, de forma simples e objetiva, como usar gRPC em C# com .NET 9.
Inclui:

- Servidor gRPC ASP.NET Core com serviço `UserService` (todos os 4 padrões: Unary, Server Streaming, Client Streaming e Bidirectional Streaming).
- Cliente console gRPC exibindo chamadas de exemplo.
- Código comentado e organizado para facilitar futura expansão (ex.: SDK para Hyperledger Fabric).

Estrutura:

```
GrpcSample/
  GrpcSample.sln
  src/
    GrpcSample.Contracts/   # .proto + geração de stubs (client/server)
    GrpcSample.Server/      # ASP.NET Core gRPC Server (UserService)
    GrpcSample.Client/      # Console gRPC Client
```

## 1) Pré-requisitos

- .NET SDK 9.0 (verifique com `dotnet --version`)
- Windows PowerShell (já incluso no ambiente)

## 2) Clonar/abrir e restaurar

No diretório raiz do repositório:

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101"
# Se ainda não estiver criado, o diretório do sample já está em GrpcSample/
# Restaurar e compilar toda a solução
dotnet build "GrpcSample\GrpcSample.sln"
```

## 3) Executar o servidor

O servidor está em `src/GrpcSample.Server` e expõe gRPC em HTTPS (ex.: `https://localhost:7156`).

```powershell
# Em um terminal
cd "GrpcSample\src\GrpcSample.Server"
dotnet run
```

Saída esperada (resumo): o Kestrel iniciará e mostrará URLs HTTPS/HTTP. O endpoint raiz `/` retorna uma mensagem informando que é um servidor gRPC. Serviços de Health Check e Reflection também estão habilitados.

## 4) Executar o cliente

Abra um segundo terminal e rode o cliente. Por padrão, ele usa `https://localhost:7156`. Se necessário, defina `GRPC_SERVER_ADDRESS`.

```powershell
# Em outro terminal
cd "..\GrpcSample.Client"
$env:GRPC_SERVER_ADDRESS = "https://localhost:7156"
dotnet run
```

O cliente executa:
- Unary: `CreateUser` e `GetUser`
- Server streaming: `ListUsers`
- Client streaming: `ImportUsers`
- Bi-di streaming: `UserEvents`

## 5) Entendendo o contrato (.proto)

Arquivo: `src/GrpcSample.Contracts/Protos/user.proto`

- Define mensagens `User`, `GetUserRequest/Response`, `CreateUserRequest/Response`, `ListUsersRequest/Response`, `ImportUsersRequest/Response`, `UserEvent`.
- Serviço `UserService` implementa 4 tipos de RPCs (Unary, Server Streaming, Client Streaming, Bidi Streaming).
- `option csharp_namespace` define o namespace C# gerado: `GrpcSample.Contracts.Users.V1`.
- O projeto `GrpcSample.Contracts` referencia `Grpc.Tools` para gerar os stubs automaticamente em build.

## 6) Servidor — visão geral

Arquivos principais:
- `Program.cs`: registra gRPC, Health Checks e Reflection, e mapeia `UserGrpcService`.
- `Services/UserGrpcService.cs`: implementação do serviço com comentários e sem uso de `var` para ensinar explicitamente os tipos.
- `InMemoryUserRepository`: padrão Repository simples para manter a organização e permitir evolução futura (DDD/Clean-friendly).

Pontos-chave:
- Retornos de erro usam `RpcException` com `StatusCode` adequado.
- Streams usam `IServerStreamWriter<T>` para envio e `IAsyncStreamReader<T>` para leitura.
- Health/Reflection ajudam em testes e observabilidade (ex.: `grpcui`, `grpcurl`).

## 7) Cliente — visão geral

Arquivo `Program.cs` demonstra como:
- Criar um canal com `GrpcChannel.ForAddress`.
- Construir o `UserServiceClient` gerado.
- Fazer chamadas Unary, Server Streaming, Client Streaming e Bi-direcional.
- Controlar cancelamento com `CancellationToken`.

Dica: ajuste `GRPC_SERVER_ADDRESS` se a porta/URL do servidor mudar.

## 8) Comandos úteis

```powershell
# Build completo
dotnet build "GrpcSample\GrpcSample.sln"

# Rodar servidor
cd "GrpcSample\src\GrpcSample.Server"; dotnet run

# Rodar cliente (em outro terminal)
cd "GrpcSample\src\GrpcSample.Client"; $env:GRPC_SERVER_ADDRESS = "https://localhost:7156"; dotnet run
```

## 9) Melhores práticas e dicas

- Tipos explícitos: evite `var` quando o objetivo é ensino e clareza.
- Contratos versionados: use pacotes/namespaces como `users.v1` e `Users.V1` para evolução do contrato.
- Repositórios/Serviços: mantenha regras de negócio fora da camada gRPC (Repository/Service patterns).
- Health/Reflection: úteis em dev; em produção, restrinja acesso conforme segurança indicada.
- SSL/TLS: gRPC usa HTTP/2. Em produção, use certificados confiáveis.
- Resiliência: adicione timeouts, retries, e políticas de backoff conforme necessário (ex.: Polly).
- Observabilidade: logue IDs de correlação e eventos relevantes; integre métricas/tracing quando evoluir.

## 10) Próximos passos (SDK Hyperledger Fabric)

- Extrair o cliente `UserService` para um pacote SDK próprio.
- Introduzir interfaces e factories para criação de canais e clientes (ex.: Abstract Factory).
- Adicionar autenticação (JWT/TLS mTLS) e interceptors para headers (ex.: rastreamento, tenancy, chaincode).
- Substituir `InMemoryUserRepository` por integrações reais (bancos, Fabric Gateways) mantendo a mesma API gRPC.

## 11) Solução de problemas

- "Não conecta": confirme a URL no servidor e a variável `GRPC_SERVER_ADDRESS` no cliente.
- Erros de certificado em dev: use `https://localhost:port` criado pelo Kestrel. Se necessário, confie no certificado de desenvolvimento do .NET (`dotnet dev-certs https --trust`).
- Porta ocupada: altere `applicationUrl` em `Properties/launchSettings.json` do servidor.

---

Este exemplo foi escrito para ser didático, direto ao ponto e pronto para servir como base de um SDK.
