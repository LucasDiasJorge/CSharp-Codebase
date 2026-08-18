# CLAUDE.md — GoodOrderApi

Mesmo domínio de `BadOrderApi`, agora seguindo Object Calisthenics. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/ObjectCalisthenics/GoodOrderApi/GoodOrderApi.csproj
dotnet run --project 07-DesignPatterns/ObjectCalisthenics/GoodOrderApi/GoodOrderApi.csproj
```

## Estrutura interna

18 arquivos onde o contraexemplo tem 5 — e o crescimento é o argumento:

- `Domain/ValueObjects/` — `Address`, `ContactInfo`, `DiscountCode`, `OrderStatus`, `PaymentMethod`. Cada primitivo com significado virou tipo próprio (regra "wrap all primitives").
- `Domain/Entities/` — `Order`, `OrderItem`, `OrderItems` (**coleção de primeira classe**, regra própria do catálogo), `Product`, `CustomerInfo`.
- `Application/Services/` — orquestração fina.
- `Api/DTOs/Mappers.cs` — fronteira explícita entre domínio e contrato HTTP.

O domínio não expõe estado mutável; o comportamento vive junto do dado.

## Pontos de atenção

- TFM `net8.0`. Pacote: `Swashbuckle.AspNetCore` 6.5.0.
- **Sem README local** — documentação em `07-DesignPatterns/ObjectCalisthenics/`.
- Ao estender, adicione ao lado bom **e** verifique se o contraste com `BadOrderApi` continua legível: o par só ensina enquanto os dois resolvem o mesmo problema.
