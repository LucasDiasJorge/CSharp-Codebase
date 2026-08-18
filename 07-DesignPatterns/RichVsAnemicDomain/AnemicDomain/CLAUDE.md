# CLAUDE.md — AnemicDomain (contraexemplo)

Console que demonstra o **anti-padrão** domínio anêmico. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/RichVsAnemicDomain/AnemicDomain/AnemicDomain.csproj
dotnet run --project 07-DesignPatterns/RichVsAnemicDomain/AnemicDomain/AnemicDomain.csproj
```

## Estrutura interna

- `Models/Order.cs` — só propriedades com get/set público. Nenhuma regra, nenhuma invariante: uma sacola de dados.
- `Services/OrderService.cs` — toda a lógica de negócio, operando sobre o objeto de fora.

A consequência a observar: qualquer código pode colocar o pedido em estado inválido, porque nada o impede. O objeto não protege a si mesmo.

## Pontos de atenção

- **Não "melhore" este projeto** — ele é metade de um par comparativo. As melhorias pertencem a `RichDomain`, que modela o mesmo pedido com comportamento encapsulado.
- TFM `net8.0`, sem dependências externas.
