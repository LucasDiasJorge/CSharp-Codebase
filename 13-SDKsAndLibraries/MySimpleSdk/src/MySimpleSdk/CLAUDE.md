# CLAUDE.md — MySimpleSdk

Biblioteca de exemplo de SDK consumível. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk/MySimpleSdk.csproj
```

Biblioteca — sem `dotnet run`. Para ver em uso: `MySimpleSdk.Demo`.

## Estrutura interna

- `Client/SdkClient.cs` — a porta de entrada do SDK.
- `Services/SdkService.cs` — a lógica.
- `Models/SdkModel.cs` — contratos de dados.
- `Exceptions/SdkException.cs` — **tipo de exceção próprio**, para que o consumidor distinga falha do SDK de falha dele.
- `MySimpleSdkExtensions.cs` — extensões de registro/uso, no estilo `AddX()`.

## Pontos de atenção

- **TFM `netstandard2.0`** — escolha deliberada para SDK: maximiza compatibilidade (roda em .NET Framework e .NET moderno), ao custo de não ter APIs recentes da linguagem/BCL. Não migre para `net*` sem entender esse trade-off, é parte da lição.
- `Newtonsoft.Json` 13.0.1 — coerente com `netstandard2.0` (`System.Text.Json` exigiria pacote adicional).
- **Sem README local**; documentação em `13-SDKsAndLibraries/MySimpleSdk/`.
- Este sample tem **solução própria** (`MySimpleSdk.sln`), além de estar na solução raiz.
