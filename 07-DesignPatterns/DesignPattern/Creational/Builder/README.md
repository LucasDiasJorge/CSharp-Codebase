# Builder Pattern - Montagem de Objetos Complexos

## Visão geral

Este projeto implementa o **Design Pattern Builder** para facilitar a criação de objetos complexos (ex: pedidos) de forma fluente e segura.

## Conceitos abordados

- **Construtores com muitos parâmetros**
- **Configuração opcional de propriedades**
- **Imutabilidade e validação**

## Objetivos de aprendizagem

- Entender como Builder Pattern - Montagem de Objetos Complexos se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
Builder/
+-- Builder.csproj
+-- Pedido.cs
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/DesignPattern/Creational/Builder/Builder.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Estrutura

- `Pedido`: Classe alvo
- `Pedido.Builder`: Classe interna para construção
- Métodos encadeados para configuração
- `Build()`: Valida e retorna o objeto

##### Exemplo de Código

```csharp
var pedido = new Pedido.Builder()
    .ComCliente("Lucas Jorge")
    .ComProduto("Notebook")
    .ComQuantidade(1)
    .ComObservacoes("Entregar após às 18h")
    .ComEntregaExpressa()
    .Build();

Console.WriteLine($"Pedido de {pedido.Produto} para {pedido.Cliente}, entrega expressa: {pedido.EntregaExpressa}");
```

##### Fluxo

1. Instancia o builder
2. Encadeia métodos para configurar
3. Chama `Build()` para criar o objeto

##### Vantagens

- Código mais legível
- Validação centralizada
- Facilidade de extensão

## Referências

- [Builder - Refactoring Guru](https://refactoring.guru/design-patterns/builder)
- [Documentação Microsoft](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#builder)
