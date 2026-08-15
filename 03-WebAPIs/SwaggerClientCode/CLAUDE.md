# CLAUDE.md — SwaggerClientCode

API que **gera código de cliente C#** a partir da própria especificação OpenAPI, via NSwag. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/SwaggerClientCode/SwaggerClientCode.csproj
dotnet run --project 03-WebAPIs/SwaggerClientCode/SwaggerClientCode.csproj
```

## Estrutura interna

O fluxo é circular e é esse o ponto do exemplo:

1. `Controllers/UserController.cs` define a API.
2. NSwag expõe o documento OpenAPI dela.
3. `ClientGenerator.cs` consome esse documento e **emite código C#** de cliente em tempo de execução.
4. `GeneratedApiClient.cs` é o resultado versionado dessa geração.

`GeneratedApiClient.cs` é artefato gerado: alterar o controller e não regerar faz o cliente divergir do contrato. Não edite o gerado à mão.

## Pontos de atenção

- TFM `net9.0`.
- **Dependências conflitantes no `.csproj`**: convivem `NSwag.*` 14.4.0, `Swashbuckle.AspNetCore` 8.1.2, `Swashbuckle` 5.6.0 (legado, era do .NET Framework) e um pacote `Swagger` 1.0.0 que não é oficial. Duas stacks OpenAPI ao mesmo tempo, mais dois pacotes provavelmente acidentais. Se este projeto falhar no build ou no startup, é aqui que se investiga primeiro — e não replique este `.csproj` como modelo.
