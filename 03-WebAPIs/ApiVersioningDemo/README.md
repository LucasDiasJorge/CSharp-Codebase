# ApiVersioningDemo

API REST que expõe a mesma funcionalidade em duas versões e demonstra as três formas de selecionar uma versão — segmento de URL, header e query string — além da descontinuação gradual de uma versão antiga.

## Visão geral

O exemplo tem dois recursos que respondem a estratégias diferentes de propósito. `/api/v{version}/products` versiona pela URL: a versão faz parte do endereço, aparece no log de acesso e pode ser roteada por um proxy. `/api/orders` mantém a URL estável e deixa a escolha para o header `X-Api-Version` ou para a query string `?api-version=`, o que preserva o endereço mas torna a versão invisível para quem só lê o log.

A v1 de produtos está marcada como depreciada. Ela continua respondendo normalmente, mas toda resposta carrega `api-deprecated-versions`, um header `Sunset` com a data de desligamento e um `Link` para o guia de migração. É assim que um cliente descobre que precisa migrar sem depender de alguém ter lido o changelog.

A diferença entre v1 e v2 de produtos é deliberada: `price` deixou de ser um número solto e virou objeto com moeda. Essa é uma quebra de contrato e foi o que exigiu uma nova versão. Já `tags`, campo novo e opcional, é uma mudança aditiva que caberia na própria v1 sem quebrar ninguém. Saber separar os dois casos é o que evita versionar por reflexo.

## Conceitos abordados

- Seleção de versão por segmento de URL, header e query string com `ApiVersionReader.Combine`.
- `DefaultApiVersion` e `AssumeDefaultVersionWhenUnspecified` como fallback para clientes que não informam versão.
- `ReportApiVersions` e os headers `api-supported-versions` / `api-deprecated-versions`.
- Descontinuação gradual com política de sunset (RFC 8594): headers `Sunset` e `Link`.
- Mudança aditiva versus mudança que quebra contrato.
- Um documento OpenAPI por versão, com `WithDocumentPerVersion`.
- Erro de versão não suportada respondido como `application/problem+json`.
- Controllers de mesmo nome em namespaces por versão, para evitar condicionais de versão no código.

## Objetivos de aprendizagem

- Escolher entre versionar na URL ou fora dela a partir das consequências de cada opção.
- Identificar quais mudanças de contrato realmente exigem uma nova versão.
- Anunciar a aposentadoria de uma versão por header, sem depender de comunicação fora da API.
- Publicar documentação separada por versão, para que cada cliente veja apenas o contrato que consome.
- Reconhecer o custo de `AssumeDefaultVersionWhenUnspecified` em uma API nova.

## Estrutura do projeto

```text
ApiVersioningDemo/
|-- Controllers/
|   |-- V1/
|   |   |-- OrdersController.cs
|   |   `-- ProductsController.cs
|   `-- V2/
|       |-- OrdersController.cs
|       `-- ProductsController.cs
|-- Models/
|   |-- OrderV1.cs
|   |-- OrderV2.cs
|   |-- ProductV1.cs
|   `-- ProductV2.cs
|-- Services/
|   `-- CatalogStore.cs
|-- Versioning/
|   `-- RequestedVersionDescriber.cs
|-- Properties/
|   `-- launchSettings.json
|-- ApiVersioningDemo.csproj
|-- ApiVersioningDemo.http
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/ApiVersioningDemo/ApiVersioningDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 03-WebAPIs/ApiVersioningDemo/ApiVersioningDemo.csproj
```

A API sobe em `http://localhost:5114` e não exige serviço externo. As requisições de exemplo estão em `ApiVersioningDemo.http`.

Verificando a depreciação pela linha de comando:

```bash
curl -i http://localhost:5114/api/v1/products
```

A resposta traz `api-deprecated-versions: 1.0`, `Sunset` e o `Link` para o guia de migração.

## Boas práticas e pontos de atenção

- Escolha uma estratégia por API e mantenha. As três coexistem aqui para comparação; misturá-las em produção multiplica os caminhos a testar e a documentar.
- Versione o contrato, não o domínio. `CatalogStore` tem uma única representação interna e cada versão projeta a sua — sem isso, cada nova versão contamina o código da anterior com condicionais.
- Controllers de mesmo nome em namespaces `V1`/`V2` mantêm cada versão isolada e produzem tags limpas no OpenAPI. Sufixar o nome da classe (`OrdersV1Controller`) gera tags truncadas como `OrdersV`.
- `AssumeDefaultVersionWhenUnspecified = true` gera o aviso do analisador **AV0016**, e o aviso está certo: em uma API nova, exigir a versão explicitamente evita que um cliente seja promovido de versão sem perceber. O exemplo mantém a opção ligada para demonstrar o fallback, e o aviso faz parte da lição.
- Depreciar não é desligar. A sequência sustentável é marcar como deprecated, anunciar por header, publicar a data de sunset e só então remover.
- `AddProblemDetails()` transforma o 400 de versão inválida em `application/problem+json` com a lista de versões suportadas. Sem ele, o cliente recebe um 400 de corpo vazio.
- A política de sunset aparece automaticamente na descrição do documento OpenAPI da versão afetada — não é preciso escrever isso à mão.

## Conteúdo complementar

Formas de seleção configuradas e seus efeitos:

| Forma | Exemplo | Vantagem | Custo |
|---|---|---|---|
| Segmento de URL | `GET /api/v2/products` | Visível no log e cacheável por URL; roteável por proxy | O endereço do recurso muda a cada versão |
| Header | `X-Api-Version: 2.0` | URL estável | Invisível no log; atrapalha cache e testes por navegador |
| Query string | `?api-version=2.0` | URL estável e fácil de testar no navegador | Polui a query; fácil de esquecer ao copiar uma URL |
| Nenhuma | `GET /api/orders` | Cliente antigo continua funcionando | O cliente não sabe qual contrato está consumindo |

Headers presentes na resposta da v1 depreciada:

```text
api-supported-versions: 2.0
api-deprecated-versions: 1.0
Sunset: Thu, 31 Dec 2026 00:00:00 GMT
Link: <https://example.com/guias/migracao-v2>; rel="sunset"; title="Guia de migracao para a v2"; type="text/html"
```

Mudanças entre versões e sua natureza:

| Recurso | v1 | v2 | Natureza |
|---|---|---|---|
| `products.price` | `349.90` | `{ "amount": 349.90, "currency": "BRL" }` | Quebra de contrato — exigiu nova versão |
| `products.tags` | ausente | `["periferico", "entrada"]` | Aditiva — caberia na v1 |
| `orders.client` | `"Ana Souza"` | renomeado para `customer` | Quebra de contrato — renomear equivale a remover |
| `orders.status` | ausente | `"confirmado"` | Aditiva |

Regra prática: acrescentar campo opcional é seguro; remover, renomear ou mudar o tipo de um campo existente não é. Clientes que desserializam estritamente quebram nos três últimos casos.

## Referências e documentação complementar

- https://github.com/dotnet/aspnet-api-versioning
- https://dotnet.github.io/aspnet-api-versioning/
- https://datatracker.ietf.org/doc/html/rfc8594
- https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi
