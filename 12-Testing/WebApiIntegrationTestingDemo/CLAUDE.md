# CLAUDE.md — WebApiIntegrationTestingDemo

Minimal API + suíte xUnit com `WebApplicationFactory`, substituição de dependência externa e verificação de contrato HTTP. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet test 12-Testing/WebApiIntegrationTestingDemo/WebApiIntegrationTestingDemo.Tests/WebApiIntegrationTestingDemo.Tests.csproj
dotnet test ... --logger "console;verbosity=detailed"   # ver a saida dos testes

# A API sozinha (provedor remoto -> 503 no POST). A porta vem do launchSettings.json.
dotnet run --project 12-Testing/WebApiIntegrationTestingDemo/WebApiIntegrationTestingDemo.csproj
```

17 testes, todos passando, em menos de 1s. Sem serviço externo.

## Estrutura interna

`Program.cs` termina com `public partial class Program;` — **é o que permite** `WebApplicationFactory<Program>`. Com top-level statements a classe gerada é interna e o projeto de teste não a alcança. Remover essa linha quebra a suíte inteira.

`Quotes/RemoteExchangeRateProvider` **lança de propósito** em vez de tentar a rede. É a dependência que o teste substitui; falhar alto transforma "esqueci de substituir" em 503 imediato, em vez de timeout.

`WebApiIntegrationTestingDemo.Tests/QuotesApiFactory` usa `ConfigureTestServices` + `RemoveAll`. `ConfigureServicesApiFactory` faz o mesmo por `ConfigureServices`, e existe **só** para o teste comparativo.

`app.UseExceptionHandler` tem `StatusCodeSelector` — ver o segundo item de Pontos de atenção. Não simplificar para `UseExceptionHandler()`.

## Pontos de atenção

- TFM `net10.0` nos dois projetos. Pacote de teste: `Microsoft.AspNetCore.Mvc.Testing`. `ConfigureTestServices` está em `Microsoft.AspNetCore.TestHost` e `RemoveAll` em `Microsoft.Extensions.DependencyInjection.Extensions` — faltando os `using`, o erro é `CS1061` sem indicar o namespace.
- **O projeto de testes aninhado exige DUAS exclusões no `.csproj` da API**: `<Compile Remove>` e **`<Content Remove>`**. O SDK Web inclui `**/*.json` como `Content`, então sem a segunda o build falha com `MSB3030` tentando copiar `.json` de `bin/` e `obj/` do projeto de testes, com caminhos aninhados absurdos. Foi exatamente o que aconteceu aqui.
- **Bug real encontrado pela suíte:** `app.UseExceptionHandler()` sem opções devolvia **500** para JSON malformado, porque ignora o `StatusCode` da `BadHttpRequestException` que o binding lança. Corrigido com `StatusCodeSelector`. **Não reverter** — o teste `Post_ComJsonMalformado_Retorna400` falha.
- **Afirmação minha refutada pela execução:** a suíte tinha um teste chamado `ConfigureServices_NaoSubstitui_EOProvedorRealEUsado`, esperando 503. O resultado foi **201 com o preço do stub**, e com `RemoveAll` sobra **uma única** implementação nos dois pontos de extensão. No .NET 10 os dois são aplicados depois das registros da aplicação. O teste atual (`ConfigureServices_E_ConfigureTestServices_TemOMesmoEfeitoNesteHost`) afirma o que foi medido. **Não reintroduzir a versão antiga.**
- **Chaves do dicionário de erros são PascalCase** (`Symbol`, `Quantity`), não camelCase — a política de nomes do serializador não se aplica a chaves de dicionário. Os `InlineData` do teste de validação dependem disso.
- `Results.ValidationProblem` produz `application/problem+json` com `errors`; `Results.Problem` produz sem `errors`. Os testes checam o `Content-Type` além do status.
- Minimal API **não valida DataAnnotations sozinha** — o `Validator.TryValidateObject` no handler é obrigatório e é o que gera o 400.
- `launchSettings.json` sobrepõe `ASPNETCORE_URLS`: ao rodar a API à mão, a porta foi 5169 mesmo pedindo 5199. Conferir a porta que o processo imprime.
- `HttpContractTests` usa `IClassFixture<QuotesApiFactory>` (uma aplicação para a classe); `DependencySubstitutionTests` e `IsolationTests` criam fábricas por teste. A diferença é deliberada e está documentada no README.
- **Fronteira com os vizinhos**: `03-WebAPIs` trata de construir APIs. `TestDoublesDemo` trata de dublê em unidade. Não adicionar aqui banco real ou Testcontainers — o exemplo é sobre o host em memória.
