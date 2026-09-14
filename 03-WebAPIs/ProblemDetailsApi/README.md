# ProblemDetailsApi

API REST que padroniza todas as respostas de erro em `application/problem+json` (RFC 9457), traduzindo falhas de validação e de domínio para os códigos HTTP correspondentes.

## Visão geral

O objetivo é que nenhum erro saia da API em formato improvisado. Um cliente que recebe 404, 409, 422 ou 500 encontra sempre a mesma estrutura: `type`, `title`, `status`, `detail`, `instance` e um `traceId` para correlacionar com o log.

A tradução acontece fora do controller. Os endpoints apenas deixam a exceção subir; uma cadeia de `IExceptionHandler` decide o status e monta o corpo. `DomainExceptionHandler` reconhece as falhas previstas do domínio e devolve o status de cada uma; `UnhandledExceptionHandler` recolhe o resto e devolve 500. Quem escreve regra de negócio não precisa saber o que é um código HTTP.

O exemplo insiste em uma distinção que costuma ser ignorada: 400 é erro de formato, 422 é regra de negócio. Um SKU fora do padrão esperado é 400 porque a requisição não bate com o contrato; um SKU válido com estoque zerado é 422, porque o servidor entendeu perfeitamente a requisição e ainda assim não pode atendê-la.

## Conceitos abordados

- `ProblemDetails` e `ValidationProblemDetails` conforme a RFC 9457.
- `AddProblemDetails` com `CustomizeProblemDetails` para campos presentes em toda resposta de erro.
- Cadeia de `IExceptionHandler` e o significado do retorno `false`.
- Exceções de domínio que carregam o próprio status HTTP e um `type` estável.
- Diferença entre 400 (formato), 422 (regra de negócio) e 409 (conflito de estado).
- Validação automática do `[ApiController]` versus `ValidationProblem` construído à mão.
- `UseStatusCodePages` para dar corpo a 404 de rota e 405 de método.
- Exposição de detalhes de exceção condicionada ao ambiente.
- `traceId` como o único dado que o usuário final precisa repassar ao suporte.

## Objetivos de aprendizagem

- Padronizar erros de uma API sem espalhar montagem de resposta pelos controllers.
- Escolher o status HTTP correto para cada tipo de falha.
- Anexar dados estruturados ao erro em `extensions`, em vez de embuti-los na mensagem.
- Entender por que o `type` deve ser uma URI estável e não a mensagem de erro.
- Evitar vazamento de informação interna em respostas de produção.

## Estrutura do projeto

```text
ProblemDetailsApi/
|-- Controllers/
|   `-- OrdersController.cs
|-- Domain/
|   |-- DomainException.cs
|   |-- InsufficientStockException.cs
|   |-- OrderAlreadyPaidException.cs
|   `-- OrderNotFoundException.cs
|-- Handlers/
|   |-- DomainExceptionHandler.cs
|   `-- UnhandledExceptionHandler.cs
|-- Models/
|   |-- CreateOrderRequest.cs
|   `-- OrderResponse.cs
|-- Services/
|   `-- OrderService.cs
|-- Properties/
|   `-- launchSettings.json
|-- Program.cs
|-- ProblemDetailsApi.csproj
|-- ProblemDetailsApi.http
`-- README.md
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/ProblemDetailsApi/ProblemDetailsApi.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 03-WebAPIs/ProblemDetailsApi/ProblemDetailsApi.csproj
```

A API sobe em `http://localhost:5158` e não exige serviço externo. Todas as requisições de exemplo estão em `ProblemDetailsApi.http`.

Para ver a diferença entre ambientes, execute fora de Development e repita o endpoint de falha:

```bash
dotnet run --project 03-WebAPIs/ProblemDetailsApi/ProblemDetailsApi.csproj --no-launch-profile
```

## Boas práticas e pontos de atenção

- Trate `type` como identificador estável de categoria de erro. É por ele que o cliente decide o que fazer — nunca pelo texto de `title` ou `detail`, que podem mudar ou ser traduzidos.
- Ponha dados estruturados em `extensions` (`orderId`, `sku`, `available`), não concatenados na mensagem. O cliente não deveria precisar de expressão regular para extrair um número do `detail`.
- Não devolva a mensagem de exceção em produção. O `UnhandledExceptionHandler` troca o texto por uma frase genérica fora de Development, mantendo o `traceId` como ponte para o log.
- Ordem de registro é ordem de execução na cadeia de handlers. O handler de fallback deve ser sempre o último; um handler que devolve `true` cedo demais engole falhas que outro trataria melhor.
- Falha de negócio é resultado esperado, não incidente. O handler de domínio registra `Warning`; só o fallback usa `Error` com a exceção inteira. Inverter isso enche o painel de alertas com fluxos normais.
- Escreva o corpo pelo `IProblemDetailsService`, não serializando à mão. É o que garante que `CustomizeProblemDetails` também se aplique aos erros vindos dos handlers.
- Sem `UseStatusCodePages`, um 404 de rota inexistente sai com corpo vazio, quebrando a promessa de que todo erro é `problem+json`.
- 422 exige que a requisição tenha sido compreendida. Se o corpo não pôde nem ser desserializado, o caso é 400.

## Conteúdo complementar

Endpoints e o que cada um demonstra:

| Requisição | Status | Origem da resposta |
|---|---|---|
| `GET /api/orders/1` | 200 | Caminho feliz |
| `GET /api/orders/99` | 404 | `OrderNotFoundException` via `DomainExceptionHandler` |
| `POST /api/orders` com SKU fora do padrão | 400 | Validação automática do `[ApiController]` |
| `POST /api/orders` com estoque zerado | 422 | `InsufficientStockException` |
| `POST /api/orders/2/payments` | 409 | `OrderAlreadyPaidException` |
| `POST /api/orders/quotes?quantity=0` | 400 | `ValidationProblem` montado no controller |
| `GET /api/orders/boom` | 500 | `UnhandledExceptionHandler` |
| `GET /api/nao-existe` | 404 | `UseStatusCodePages` |
| `DELETE /api/orders/1` | 405 | `UseStatusCodePages` |

Erro de domínio com dados estruturados:

```json
{
  "type": "https://example.com/erros/estoque-insuficiente",
  "title": "Regra de negocio violada",
  "status": 422,
  "detail": "Estoque insuficiente para o SKU XYZ-9999: pedidas 5, disponiveis 0.",
  "instance": "POST /api/orders",
  "sku": "XYZ-9999",
  "requested": 5,
  "available": 0,
  "traceId": "00-738219b693b1f5e3b055dd91ac801224-680a5c9ef4993d66-00"
}
```

Erro de validação, com o dicionário `errors` por campo:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "POST /api/orders",
  "errors": {
    "Sku": ["O SKU deve seguir o formato AAA-0000."],
    "Quantity": ["A quantidade deve estar entre 1 e 100."],
    "CustomerEmail": ["O cliente deve ser um e-mail valido."]
  },
  "traceId": "00-a9fb464478bea3829b329dc1ebb03f12-0b84133453c5a0b4-00"
}
```

Mesma falha interna, respostas diferentes por ambiente:

```text
Development: "detail": "Falha inesperada em dependencia interna."
             "exceptionType": "System.InvalidOperationException"

Production:  "detail": "A requisicao nao pode ser concluida. Use o traceId ao acionar o suporte."
             (sem exceptionType)
```

Como escolher o status:

| Situação | Status | Critério |
|---|---|---|
| Corpo malformado, campo com tipo ou formato errado | 400 | O servidor não conseguiu interpretar a requisição |
| Requisição compreendida, regra de negócio impede | 422 | Entendeu, mas não pode processar |
| Conflita com o estado atual do recurso | 409 | Repetir sem reconsultar não vai funcionar |
| Recurso não existe | 404 | Nada a apontar |
| Falha do servidor | 500 | A culpa não é do cliente; não expor detalhes |

Relação com os vizinhos da trilha: `FluentValidationUserApi` cobre a construção das regras de validação; este projeto cobre o que a API responde quando uma regra — de formato ou de negócio — falha. `ApiVersioningDemo` usa `AddProblemDetails()` apenas para dar corpo ao erro de versão inválida.

## Referências e documentação complementar

- https://datatracker.ietf.org/doc/html/rfc9457
- https://learn.microsoft.com/aspnet/core/fundamentals/error-handling
- https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandler
- https://learn.microsoft.com/aspnet/core/web-api/handle-errors
