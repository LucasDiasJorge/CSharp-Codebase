# CLAUDE.md — RichDomain

Console com o mesmo pedido de `AnemicDomain`, agora com domínio rico. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/RichVsAnemicDomain/RichDomain/RichDomain.csproj
dotnet run --project 07-DesignPatterns/RichVsAnemicDomain/RichDomain/RichDomain.csproj
```

## Estrutura interna

- `Models/Order.cs` — setters fechados, mudanças de estado só por métodos que **validam a invariante antes de aplicar**. O objeto recusa estado inválido.
- `Services/OrderApplicationService.cs` — camada de aplicação **fina**: orquestra e delega, não decide regra. Compare com o `OrderService` gordo do lado anêmico.

## Pontos de atenção

- TFM `net8.0`, sem dependências externas.
- Ao estender, resista a adicionar regra no application service: a regra pertence à entidade. É exatamente a fronteira que o par de projetos existe para ensinar.
- Mesma tese, em escala maior: `07-DesignPatterns/ObjectCalisthenics/GoodOrderApi`.
