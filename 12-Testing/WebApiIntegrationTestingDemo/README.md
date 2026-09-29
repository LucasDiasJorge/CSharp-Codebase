# WebApiIntegrationTestingDemo

Minimal API e suíte xUnit que a testam em memória com `WebApplicationFactory`, substituindo a dependência externa e verificando o contrato HTTP de ponta a ponta.

## Visão geral

`WebApplicationFactory<Program>` sobe a aplicação **de verdade**: o mesmo `Program.cs`, o mesmo pipeline, o mesmo serializador, o mesmo tratamento de erro. Não há porta TCP — o `HttpClient` devolvido por `CreateClient()` despacha direto no pipeline, e a suíte inteira roda em menos de meio segundo.

O que se ganha com isso é a camada que teste de unidade não vê. O handler de um teste de unidade devolve um objeto; aqui devolve **resposta HTTP**, com status, header e corpo serializado. Três exemplos que só aparecem desse lado:

- `POST /quotes` devolve **201** com `Location`, e o teste **segue o header** — o contrato não é "existe um header", é que ir nele funciona.
- O JSON sai em **camelCase** (`unitPrice`), mas as chaves do dicionário de erros de validação saem em **PascalCase** (`Symbol`) — a política de nomes vale para propriedades, não para chaves de dicionário.
- `/quotes/{id:guid}` com um id que não é GUID devolve **404**, não 400: a restrição de rota rejeita antes do handler existir.

Dois achados deste exemplo valem mais que o resto. O primeiro é um bug encontrado pela suíte: `app.UseExceptionHandler()` **sem opções** transforma qualquer exceção em 500, inclusive a `BadHttpRequestException` que o binding lança para JSON malformado — e que deveria ser 400. A correção é o `StatusCodeSelector`, e nenhum teste de unidade teria visto isso.

O segundo é uma crença desmentida: conta-se que substituir dependência em `ConfigureServices` "não funciona" porque ele roda antes das registros da aplicação. **Medido no .NET 10, não se reproduz** — os dois pontos de extensão são aplicados depois, e `RemoveAll` funciona nos dois. `ConfigureTestServices` continua sendo o certo, por ser a garantia documentada; só não falha como se conta.

## Conceitos abordados

- `WebApplicationFactory<TEntryPoint>` e execução em memória, sem porta.
- `public partial class Program` como âncora de tipo com top-level statements.
- `ConfigureTestServices` e `RemoveAll` para substituir dependência externa.
- `WithWebHostBuilder` para substituição válida em um único teste.
- Contrato HTTP: 201 + `Location`, 400, 404, 405, 415, 503.
- `ProblemDetails` e `application/problem+json`.
- camelCase em propriedades contra PascalCase em chaves de dicionário.
- Enum serializado como string por configuração explícita.
- Isolamento: uma fábrica é uma aplicação, com seus próprios singletons.

## Objetivos de aprendizagem

- Escrever teste de integração que exercite o pipeline real da aplicação.
- Substituir só a dependência externa, mantendo tudo o mais igual à produção.
- Verificar contrato HTTP, e não apenas o valor de retorno do handler.
- Reconhecer o que o teste de unidade do handler nunca vai pegar.
- Controlar isolamento de estado entre testes de forma deliberada.

## Estrutura do projeto

```text
WebApiIntegrationTestingDemo/
|-- Quotes/
|   `-- QuoteContracts.cs
|-- WebApiIntegrationTestingDemo.Tests/
|   |-- DependencySubstitutionTests.cs
|   |-- HttpContractTests.cs
|   |-- QuotesApiFactory.cs
|   |-- StubExchangeRateProvider.cs
|   `-- WebApiIntegrationTestingDemo.Tests.csproj
|-- Program.cs
|-- WebApiIntegrationTestingDemo.csproj
`-- README.md
```

## Como executar

```bash
dotnet test 12-Testing/WebApiIntegrationTestingDemo/WebApiIntegrationTestingDemo.Tests/WebApiIntegrationTestingDemo.Tests.csproj
```

Com a saída dos testes que imprimem o que encontraram:

```bash
dotnet test 12-Testing/WebApiIntegrationTestingDemo/WebApiIntegrationTestingDemo.Tests/WebApiIntegrationTestingDemo.Tests.csproj --logger "console;verbosity=detailed"
```

A API também roda sozinha, mas aí o provedor de cotação é o **remoto**, que exige rede:

```bash
dotnet run --project 12-Testing/WebApiIntegrationTestingDemo/WebApiIntegrationTestingDemo.csproj
# GET  /quotes/{guid}  -> 404 application/problem+json
# POST /quotes         -> 503 "o provedor remoto precisa de rede para cotar 'PETR'"
```

São 17 testes, todos passando. Não exige serviço externo.

## Boas práticas e pontos de atenção

- Substitua **só** a fronteira externa. Trocar o repositório, o serializador ou o pipeline transforma o teste de integração em teste de unidade caro, que passa sem provar nada sobre a aplicação real.
- Use `ConfigureTestServices`, não `ConfigureServices`. A diferença de comportamento medida aqui foi nenhuma, mas a garantia de ordem é documentada só para o primeiro — e depender de comportamento não documentado é como esse tipo de teste começa a falhar entre versões.
- Chame `RemoveAll<T>()` antes de registrar o substituto. Sem isso, `IEnumerable<T>` entrega as duas implementações, e o bug só aparece no dia em que alguém injetar a coleção.
- Faça a dependência de produção **falhar alto** se for chamada em teste. O `RemoteExchangeRateProvider` lança na hora em vez de tentar a rede e travar por timeout — isso transforma "esqueci de substituir" em erro imediato.
- Verifique o contrato, não só o status. `Location` que não pode ser seguido, `problem+json` com corpo vazio e enum como número são contratos quebrados com 2xx.
- Trate `UseExceptionHandler` com cuidado: sem `StatusCodeSelector`, ele apaga o status que a exceção carregava. Foi esse o bug que a suíte encontrou aqui.
- Decida o isolamento de propósito. `IClassFixture<T>` compartilha uma aplicação entre os testes da classe (rápido, estado compartilhado); instanciar a fábrica por teste isola (lento, previsível). As duas formas estão nesta suíte, e a escolha depende do estado envolvido.
- Não use `WebApplicationFactory` para testar regra de negócio. Regra se testa em unidade; aqui se testa borda HTTP, roteamento, serialização e composição.
- `launchSettings.json` sobrepõe `ASPNETCORE_URLS` no `dotnet run`. Ao rodar a API à mão, confira a porta que ela imprime em vez de assumir a que você pediu.

## Conteúdo complementar

**1. O contrato verificado**, por teste:

| Requisição | Resposta | O que prova |
|---|---|---|
| `POST /quotes` válido | **201** + `Location` | seguir o `Location` devolve 200 com o recurso |
| `POST /quotes` símbolo/quantidade inválidos | **400** `problem+json` | erros por campo em `errors` |
| `GET /quotes/{guid}` inexistente | **404** `problem+json` | `status: 404` no corpo |
| `GET /quotes/nao-e-guid` | **404** | restrição de rota rejeita antes do handler |
| `POST` com `x-www-form-urlencoded` | **415** | negociação de conteúdo |
| `DELETE /quotes/{guid}` | **405** | rota existe, método não |
| `POST` com JSON malformado | **400** | `StatusCodeSelector` preserva o status |
| `POST` com provedor remoto ativo | **503** | falha de dependência externa não é 500 |

**2. O bug que a suíte encontrou**:

```text
antes:  app.UseExceptionHandler();
        POST /quotes com '{"symbol": "PETR",'  ->  500 Internal Server Error

depois: app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            StatusCodeSelector = exception => exception is BadHttpRequestException bad
                ? bad.StatusCode
                : StatusCodes.Status500InternalServerError,
        });
        POST /quotes com '{"symbol": "PETR",'  ->  400 Bad Request
```

O binding do Minimal API lança `BadHttpRequestException` com `StatusCode = 400`. O `UseExceptionHandler` sem opções ignora esse status e devolve 500 — cliente recebendo "erro do servidor" para um pedido que ele mesmo enviou errado.

**3. camelCase não vale para tudo**:

```json
{"id":"...","symbol":"PETR","quantity":4,"unitPrice":38.50,"total":154.00,"status":"Open"}
```

```json
{"title":"One or more validation errors occurred.","status":400,
 "errors":{"Symbol":["symbol deve ter de 3 a 5 letras maiusculas"]}}
```

`unitPrice` em camelCase (propriedade), `Symbol` em PascalCase (**chave de dicionário**). Para uniformizar, é preciso configurar `DictionaryKeyPolicy` — e o único lugar onde essa diferença aparece é num teste que olha o JSON.

**4. `ConfigureServices` contra `ConfigureTestServices`**, medido:

```text
ConfigureServices     -> StubExchangeRateProvider
ConfigureTestServices -> StubExchangeRateProvider
```

Os dois substituem, e nos dois o `RemoveAll` deixa uma única implementação. A primeira versão desta suíte afirmava que o primeiro falharia, e o teste reprovou a afirmação: a API respondeu **201 com o preço do stub**, não 503.

**5. Isolamento**:

| Arranjo | Estado |
|---|---|
| Duas fábricas | aplicações distintas: o recurso criado em uma dá 404 na outra |
| Dois `CreateClient()` da mesma fábrica | mesma aplicação: mesmo singleton, mesmo repositório |

É a causa mais comum de teste que passa sozinho e falha na suíte.

Relação com os vizinhos: `TestDoublesDemo` cobre substituição de colaborador em unidade; aqui a substituição acontece pelo contêiner de DI da aplicação real. A trilha `03-WebAPIs` trata de construir as APIs; este projeto trata de testá-las.

## Referências e documentação complementar

- https://learn.microsoft.com/aspnet/core/test/integration-tests
- https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1
- https://learn.microsoft.com/aspnet/core/fundamentals/error-handling
- https://learn.microsoft.com/dotnet/api/system.text.json.jsonserializeroptions.dictionarykeypolicy
