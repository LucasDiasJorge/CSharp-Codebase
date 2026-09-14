# CLAUDE.md — ProblemDetailsApi

API que padroniza erros em `application/problem+json` (RFC 9457) e traduz falhas de validação e de domínio para os status HTTP corretos. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/ProblemDetailsApi/ProblemDetailsApi.csproj
dotnet run --project 03-WebAPIs/ProblemDetailsApi/ProblemDetailsApi.csproj
```

Sobe em `http://localhost:5158`. Os nove caminhos de erro estão prontos em `ProblemDetailsApi.http`.

## Estrutura interna

`Domain/DomainException` é a peça central: cada falha prevista carrega o próprio `StatusCode`, um `ErrorType` (URI estável) e `Extensions` com os dados estruturados da falha. As três subclasses cobrem 404 (`OrderNotFoundException`), 409 (`OrderAlreadyPaidException`) e 422 (`InsufficientStockException`).

`Handlers/` tem a cadeia: `DomainExceptionHandler` devolve `false` para o que não for `DomainException`, passando para `UnhandledExceptionHandler`, que fecha em 500. **A ordem em `Program.cs` é a ordem de execução** — o fallback precisa ser o último `AddExceptionHandler`.

Ambos escrevem pelo `IProblemDetailsService.TryWriteAsync`, não serializando à mão; é o que faz `CustomizeProblemDetails` valer também para eles.

`Controllers/OrdersController` não monta nenhum ProblemDetails de domínio — só deixa a exceção subir. A exceção à regra é `Quote`, que usa `ValidationProblem(ModelStateDictionary)` de propósito, para mostrar a alternativa sem exceção.

## Pontos de atenção

- TFM `net10.0` (a maior parte da trilha é `net9.0`): a máquina não tem o runtime ASP.NET Core 9.0 instalado e este sample precisa ser executado. Mesma decisão de [ApiVersioningDemo](../ApiVersioningDemo/CLAUDE.md).
- Sem pacote externo além do `Microsoft.AspNetCore.OpenApi` do template. `ProblemDetails`, `IExceptionHandler` e `IProblemDetailsService` são do framework.
- `app.UseStatusCodePages()` é o que dá corpo ao 404 de rota inexistente e ao 405. Removê-lo quebra silenciosamente a promessa de que todo erro é `problem+json` — e só nos dois caminhos que não passam por controller, que são fáceis de não testar.
- **Verificado empiricamente nesta versão:** `CustomizeProblemDetails` também se aplica ao 400 automático do `[ApiController]` e às respostas do `UseStatusCodePages` — `traceId` e `instance` aparecem nos nove caminhos. Não presuma o contrário ao mexer aqui; teste.
- O comportamento do 500 **muda por ambiente**: em Development o `detail` traz a mensagem real e há `exceptionType`; fora dele, frase genérica e nada de tipo. Ao testar, confirme os dois (`--no-launch-profile` com `ASPNETCORE_ENVIRONMENT=Production`), porque o caminho arriscado é justamente o que não roda no dia a dia.
- Os `type` apontam para `https://example.com/erros/...`, que não existe. É proposital — o campo é identificador, não link a ser seguido. Não troque por URLs reais nem por texto livre.
- Estado em memória: `OrderService` guarda os pedidos pagos em um dicionário de instância singleton. O 409 do pedido 2 funciona sempre; um pedido 1 pago não volta a ficar aberto sem reiniciar o processo.
- **Fronteira com os vizinhos**: `FluentValidationUserApi` trata de como escrever regras de validação; aqui o assunto é o formato da resposta quando elas falham. Não migre este projeto para FluentValidation — o 400 automático do `[ApiController]` com Data Annotations é parte do que está sendo demonstrado.
