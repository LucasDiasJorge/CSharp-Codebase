# VerticalSliceMinimalApi

Resumo curto de uma Minimal API organizada por vertical slice, onde cada funcionalidade agrupa endpoint, contrato e regra de aplicacao no mesmo contexto.

## Visao geral

Este projeto demonstra como estruturar uma Minimal API com arquitetura de pastas em vertical slice.
Em vez de separar por camadas tecnicas extensas, cada fatia funcional (`Create`, `GetById`, `List`) concentra os arquivos necessarios para atender um caso de uso especifico.

A API utiliza repositorio em memoria para manter o foco no desenho de codigo, no fluxo HTTP e na separacao de responsabilidades por feature.

## Conceitos abordados

- Minimal API com ASP.NET Core.
- Organizacao por vertical slice em `Features/Orders/*`.
- Separacao entre dominio, infraestrutura e fatias de uso.
- Injecao de dependencias para handlers por feature.
- Contratos HTTP explicitos com request/response tipados.

## Objetivos de aprendizagem

- Entender quando usar vertical slice em APIs pequenas e medias.
- Criar endpoints orientados a caso de uso.
- Evitar acoplamento desnecessario entre funcionalidades.
- Estruturar um sample didatico com evolucao incremental por feature.

## Estrutura do projeto

```text
VerticalSliceMinimalApi/
|-- Domain/
|   `-- Order.cs
|-- Features/
|   `-- Orders/
|       |-- Create/
|       |-- GetById/
|       `-- List/
|-- Infrastructure/
|   `-- Orders/
|       |-- IOrderRepository.cs
|       `-- InMemoryOrderRepository.cs
|-- Program.cs
`-- VerticalSliceMinimalApi.csproj
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/VerticalSliceMinimalApi/VerticalSliceMinimalApi.csproj
```

## Boas praticas e pontos de atencao

- Mantenha cada slice com responsabilidade unica e foco em um caso de uso.
- Preserve contratos de entrada e saida por feature para reduzir impacto de mudancas.
- Em cenarios reais, substitua o repositorio em memoria por persistencia apropriada.
- Nao concentre regras de negocio no `Program.cs`; prefira handlers por slice.

## Conteudo complementar

### Endpoints disponiveis

- `POST /orders` - cria um pedido com `customerName` e `totalAmount`.
- `GET /orders/{id}` - busca um pedido pelo identificador.
- `GET /orders` - lista os pedidos cadastrados.

### Exemplo de payload para criacao

```json
{
  "customerName": "Lucas",
  "totalAmount": 120.50
}
```

## Referencias e documentacao complementar

- https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#vertical-slice-architecture
