# CLAUDE.md — BadOrderApi (contraexemplo)

API que **viola deliberadamente** as regras de Object Calisthenics. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/ObjectCalisthenics/BadOrderApi/BadOrderApi.csproj
dotnet run --project 07-DesignPatterns/ObjectCalisthenics/BadOrderApi/BadOrderApi.csproj
```

## Estrutura interna

Cinco arquivos achatados: `Models.cs`, `DTOs.cs`, `Services/OrderService.cs`, `Controllers/OrdersController.cs`, `Program.cs`. Tipos primitivos no lugar de value objects, condicionais aninhadas, getters expostos e um service que concentra tudo.

**Este código é ruim de propósito.** Ele só existe como termo de comparação para `GoodOrderApi`, que resolve o mesmo domínio seguindo as regras. Leia os dois lado a lado.

## Pontos de atenção

- **Não "corrija" este projeto.** Melhorar o código aqui destrói o contraste que é a razão de ele existir. Melhorias pertencem a `GoodOrderApi`.
- TFM `net8.0` (a maioria da trilha está em `net9.0`). Pacote: `Swashbuckle.AspNetCore` 6.5.0.
- **Sem README local** — documentação em `07-DesignPatterns/ObjectCalisthenics/`.
