# CLAUDE.md — GrpcSample.Client

Cliente console que consome o servidor gRPC do sample. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/GrpcSample/src/GrpcSample.Client/GrpcSample.Client.csproj

# 1) em um terminal:
dotnet run --project 03-WebAPIs/GrpcSample/src/GrpcSample.Server/GrpcSample.Server.csproj
# 2) em outro:
dotnet run --project 03-WebAPIs/GrpcSample/src/GrpcSample.Client/GrpcSample.Client.csproj
```

## Estrutura interna

`Program.cs` abre um `GrpcChannel` para o endereço do servidor, instancia o client **gerado** a partir de `GrpcSample.Contracts` e faz as chamadas. Não há stub escrito à mão: o tipo do client vem da geração do `.proto`.

## Pontos de atenção

- TFM `net9.0`. Pacotes: `Grpc.Net.Client` 2.65.0, `Google.Protobuf` 3.27.0. Referencia `GrpcSample.Contracts`.
- O endereço do servidor está fixo no código; se você mudar a porta em `launchSettings.json` do servidor, atualize aqui também.
- **Sem README local** — documentação em `03-WebAPIs/GrpcSample/`.
