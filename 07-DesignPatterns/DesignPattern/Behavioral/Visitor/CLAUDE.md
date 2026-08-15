# CLAUDE.md — Visitor

Console: operações novas sobre uma hierarquia fechada de itens. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Behavioral/Visitor/Visitor.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/Visitor/Visitor.csproj
```

## Estrutura interna

- `IElement.cs` + `Book.cs` / `Dvd.cs` — a hierarquia estável.
- `IVisitor.cs` + `PriceVisitor.cs` / `ShippingVisitor.cs` — as operações, cada uma em sua classe.

O trade-off que o exemplo expõe: **adicionar operação é barato** (nova classe visitor), **adicionar tipo é caro** (mexe em todos os visitors). Use Visitor quando os tipos são estáveis e as operações crescem — no cenário inverso, ele é a escolha errada.

## Pontos de atenção

- **TFM `net7.0`** — fora de suporte e diferente do resto da pasta `DesignPattern/` (`net9.0`). Preserve ao editar.
- README local em inglês, fora do padrão PT-BR do repositório. Mantenha o idioma do arquivo.
- Sem dependências externas.
