<!-- README padronizado (versão condensada) -->
# GrpcSample (Server + Client)

Exemplo completo de gRPC em .NET 9 cobrindo os 4 tipos de RPC (Unary, Server Streaming, Client Streaming, Bidirectional). Código com tipos explícitos e separação em projetos: contratos, servidor e cliente.

## 1. Estrutura
```
GrpcSample/
  src/
    GrpcSample.Contracts/ (user.proto + stubs via Grpc.Tools)
    GrpcSample.Server/    (UserService + infra gRPC, Health, Reflection)
    GrpcSample.Client/    (Console demonstrativo)
```

## 2. Objetivos Didáticos
- Compreender geração automática de stubs a partir de `.proto`.
- Ilustrar cada padrão de stream.
- Enfatizar separação de responsabilidades (Service vs Repository).
- Preparar terreno para evolução (versionamento de contratos, SDK futuro).

## 3. Execução Rápida
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GrpcSample\src\GrpcSample.Server"
dotnet run

# Novo terminal
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\GrpcSample\src\GrpcSample.Client"
$env:GRPC_SERVER_ADDRESS = "https://localhost:7156"
dotnet run
```

## 4. Contrato (user.proto)
Define mensagens `User`, requests/responses específicas e serviço `UserService` com 4 estilos:
| Tipo | RPC | Descrição |
|------|-----|-----------|
| Unary | CreateUser / GetUser | Request/Response simples |
| Server Streaming | ListUsers | Servidor envia múltiplos itens |
| Client Streaming | ImportUsers | Cliente envia stream, recebe resumo |
| Bidi Streaming | UserEvents | Interação contínua bidirecional |

Namespace gerado configurado por `option csharp_namespace`. Stubs via pacote `Grpc.Tools`.

## 5. Servidor (Principais Pontos)
- Registro: `builder.Services.AddGrpc()` + Reflection/Health Checks para debug.
- Implementação em `UserGrpcService` usando `IServerStreamWriter<T>` / `IAsyncStreamReader<T>`.
- Erros semânticos tratados com `RpcException` + `StatusCode` adequado.

## 6. Cliente (Console)
Usa `GrpcChannel.ForAddress` (HTTP/2). Demonstra sequencialmente cada chamada, apresentando logs de fluxo. Variável `GRPC_SERVER_ADDRESS` permite alterar endpoint sem recompilar.

## 7. Boas Práticas Aplicadas
- Tipos explícitos (clareza didática – sem `var`).
- Repositório em memória isolando persistência futura.
- Possibilidade de versionamento do contrato (ex.: `users.v1`).
- Health + Reflection somente em desenvolvimento.
- Facilidade para interceptors (autenticação, tracing) em evolução.

## 8. Próximos Passos Sugeridos
- Introduzir autenticação JWT ou mTLS.
- Adicionar interceptors para correlação e retry/backoff (Polly + gRPC CallCredentials).
- Criar projeto SDK que encapsule criação de canais e policies.
- Persistência real (SQL/NoSQL) substituindo repositório em memória.

## 9. Solução de Problemas
| Sintoma | Ação |
|---------|------|
| Canal não conecta | Verificar URL/porta exibida no console do servidor |
| Erro TLS dev | Rodar `dotnet dev-certs https --trust` |
| Porta ocupada | Alterar `launchSettings.json` (applicationUrl) |
| Streams não retornam | Validar cancelamento / tokens e firewall local |

## 10. Aprendizados Esperados
Entender ciclo contrato → geração → implementação → consumo, além das diferenças práticas entre os padrões de RPC e considerações de expansão (observabilidade, segurança, versionamento).

---
README original detalhado substituído por versão condensada padronizada.
