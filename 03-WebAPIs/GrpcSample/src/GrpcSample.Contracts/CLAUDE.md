# CLAUDE.md — GrpcSample.Contracts

Biblioteca de contratos Protobuf compartilhada entre cliente e servidor gRPC. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/GrpcSample/src/GrpcSample.Contracts/GrpcSample.Contracts.csproj
```

Biblioteca — não é executável. Para rodar o sample, ver os `CLAUDE.md` de `GrpcSample.Server` e `GrpcSample.Client`.

## Estrutura interna

- `Protos/user.proto` — **a fonte de verdade** do contrato. Serviço e mensagens são definidos aqui.
- `Grpc.Tools` gera o código C# em tempo de build a partir do `.proto`.
- `Protos/Generated/Protos/User.cs` e `UserGrpc.cs` — saída gerada que está **versionada no repositório**. Não edite à mão: altere o `.proto` e rebuilde, senão gerado e contrato divergem silenciosamente.
- `Class1.cs` — resíduo do template `classlib`, sem uso.

Cliente e servidor referenciam este projeto, garantindo um único contrato para os dois lados.

## Pontos de atenção

- TFM `net9.0`. Pacotes: `Grpc.Tools` 2.65.0, `Google.Protobuf` 3.27.0, `Grpc.Core.Api` 2.65.0.
- **Sem README local**; a documentação do sample está em `03-WebAPIs/GrpcSample/`.
- Há um segundo `.proto` no servidor (`Protos/greet.proto`) que **não** vive aqui — nem todo contrato do sample está centralizado.
