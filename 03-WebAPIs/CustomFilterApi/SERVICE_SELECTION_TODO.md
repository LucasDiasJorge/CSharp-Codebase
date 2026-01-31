# Plano de Ação — Seleção de Service via Filter (CustomFilterApi)

Este arquivo descreve um plano de ação detalhado (TODO) para implementar a seleção dinâmica de uma implementação de serviço dentro de um filtro (`LogPropertyFilter`) e disponibilizar a instância escolhida para os controllers.

Objetivo: com base em verificações no `OnActionExecuting` do filtro, decidir qual implementação de `IBusinessService` deve ser usada para processar a requisição atual, e tornar essa instância acessível ao controller de forma segura e testável.

---

## Resumo da estratégia

- Criar uma interface `IBusinessService` e duas implementações (por exemplo, `BusinessServiceA` e `BusinessServiceB`).
- Criar um accessor scoped `ISelectedServiceAccessor` que armazena a instância escolhida durante o ciclo da requisição.
- No filtro (`LogPropertyFilter.OnActionExecuting`) inspecionar o payload (ex.: `UserDto.Age` ou outro indicador) e resolver a implementação adequada via `context.HttpContext.RequestServices` (DI container).
- Atribuir a instância ao `ISelectedServiceAccessor.Selected`.
- O controller injeta `ISelectedServiceAccessor` e usa `Selected` para executar a lógica apropriada.

---

## TODO (itens acionáveis)

1. Definir interfaces e serviços — `in-progress`
   - Arquivos a criar:
     - `Services/IBusinessService.cs`
     - `Services/BusinessServiceA.cs`
     - `Services/BusinessServiceB.cs`
   - Contrato:
     ```csharp
     public interface IBusinessService
     {
         string Execute(object payload);
     }
     ```
   - Critério de aceitação: os arquivos compilam e cada implementação retorna uma string identificadora (`"A"` ou `"B"`) para testes rápidos.

2. Criar accessor selecionado — `not-started`
   - Arquivo:
     - `Services/SelectedServiceAccessor.cs`
   - Contrato:
     ```csharp
     public interface ISelectedServiceAccessor
     {
         IBusinessService? Selected { get; set; }
     }
     public class SelectedServiceAccessor : ISelectedServiceAccessor
     {
         public IBusinessService? Selected { get; set; }
     }
     ```
   - Critério de aceitação: accessor registrado como Scoped e injetável.

3. Registrar serviços no DI — `not-started`
   - Atualizar `Program.cs`:
     - `builder.Services.AddScoped<BusinessServiceA>();`
     - `builder.Services.AddScoped<BusinessServiceB>();`
     - `builder.Services.AddScoped<ISelectedServiceAccessor, SelectedServiceAccessor>();`
     - `builder.Services.AddScoped<LogPropertyFilter>();` (já registrado, confirmar)
   - Critério de aceitação: `dotnet build` passa.

4. Modificar `LogPropertyFilter` para decidir serviço — `not-started`
   - Alterações em `OnActionExecuting`:
     - Inspecionar `context.ActionArguments` para encontrar o payload (por exemplo chave `"user"` com tipo `UserDto`).
     - Usar a lógica de decisão (ex.: se `user.Age < 30` escolha `BusinessServiceA`, senão `BusinessServiceB`).
     - Resolver a instância com `context.HttpContext.RequestServices.GetRequiredService<BusinessServiceA>()` ou `GetRequiredService<BusinessServiceB>()`.
     - Gravar a instância em `accessor.Selected`.
   - Observação de design: evite `new` manual; resolva via DI para respeitar scopes e facilitar testes.
   - Critério de aceitação: após a execução do filtro, `ISelectedServiceAccessor.Selected` não é nulo para requisições com payload válido.

5. Atualizar controllers para usar accessor — `not-started`
   - Atualizar `UsersController`:
     - Injetar `ISelectedServiceAccessor accessor` no construtor.
     - Em `CreateUser`, usar `accessor.Selected?.Execute(user)` para obter o resultado.
   - Critério de aceitação: endpoint retorna `ServiceResult` no body quando o serviço é selecionado.

6. Adicionar testes unitários/integração — `not-started`
   - Criar um projeto de testes `CustomFilterApi.Tests` (xUnit).
   - Testes sugeridos:
     - Unit: Mock `ActionExecutingContext` + `RequestServices` para validar que o filtro define `accessor.Selected` corretamente.
     - Integration: Arrancar a API em memória (`WebApplicationFactory`) e fazer POSTs testando logs e respostas.
   - Critério de aceitação: `dotnet test` passa localmente (pelo menos os testes unitários básicos).

7. Atualizar documentação e exemplos — `not-started`
   - Adicionar seção no `README.md` e criar `SERVICE_SELECTION_TODO.md` (este arquivo)
   - Exemplos de requisição (cURL) e logs esperados.

8. Smoke test e validação final — `not-started`
   - Executar `dotnet build`, `dotnet run` e realizar requisições via `curl` ou `CustomFilterApi.http`.
   - Validar logs para confirmar a seleção do serviço.

---

## Exemplo de código (trechos chave)

### Trecho: resolver serviço no filtro
```csharp
// Dentro de OnActionExecuting
if (context.ActionArguments.TryGetValue("user", out var userObj) && userObj != null)
{
    var accessor = context.HttpContext.RequestServices.GetRequiredService<ISelectedServiceAccessor>();

    var ageProp = userObj.GetType().GetProperty("Age");
    var age = ageProp != null ? (int?)(ageProp.GetValue(userObj) as int?) : null;

    if (age.HasValue && age.Value < 30)
        accessor.Selected = context.HttpContext.RequestServices.GetRequiredService<BusinessServiceA>();
    else
        accessor.Selected = context.HttpContext.RequestServices.GetRequiredService<BusinessServiceB>();
}
```

### Trecho: controller usando accessor
```csharp
public class UsersController : ControllerBase
{
    private readonly ISelectedServiceAccessor _accessor;

    public UsersController(ILogger<UsersController> logger, ISelectedServiceAccessor accessor)
    {
        _logger = logger;
        _accessor = accessor;
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] UserDto user)
    {
        var result = _accessor.Selected?.Execute(user) ?? "No service selected";
        return Ok(new { Message = "Usuário criado", ServiceResult = result });
    }
}
```

---

## Observações e riscos

- Evite lógica complexa no filtro. O filtro deve decidir qual serviço usar, mas a execução da lógica de negócio deve ficar nas classes de serviço para manter SRP.
- Use `IServiceProvider` para resolver serviços e evite `Activator.CreateInstance` ou `new` para classes que dependem de DI.
- Para cenários com muitas implementações dinâmicas, considere usar uma `IFactory<T>` com registro por key ou Strategy pattern.
- Logging e tracing: registre qual serviço foi selecionado para auditoria.

---

## Próximos passos

- Caso queira, eu posso:
  1. Gerar os arquivos de serviço (`IBusinessService`, `BusinessServiceA`, `BusinessServiceB`).
  2. Implementar o `SelectedServiceAccessor`.
  3. Atualizar `Program.cs` para registrar tudo no DI.
  4. Modificar `LogPropertyFilter` e `UsersController` conforme descrito.

Responda com "Gerar agora" para eu aplicar as mudanças no repositório e executar a validação (build + smoke test).