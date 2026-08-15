# CLAUDE.md — Serialization

Console: técnicas de serialização, incluindo o formato binário MessagePack. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/Serialization/Serialization.csproj
dotnet run --project 11-Utilities/Serialization/Serialization.csproj
```

## Estrutura interna

- `Pessoa.cs` — o modelo serializado; precisa dos atributos do MessagePack (`[MessagePackObject]` e `[Key]`) para o formato binário.
- `Program.cs` — comparação entre formatos e tratamento de erro de desserialização.

O contraste com JSON é o ponto: MessagePack produz payload menor e mais rápido, ao custo de não ser legível por humanos e de depender dos índices de `[Key]` — **mudar a numeração quebra a compatibilidade** com dados já serializados.

## Pontos de atenção

- TFM `net9.0`. `MessagePack` 3.1.3.
- Para XML, ver `ClassToXml` e `XmlBasics` na mesma trilha; para JSON, `01-Fundamentals/Course`.
