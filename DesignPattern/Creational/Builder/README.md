# Builder Pattern - Montagem de Objetos Complexos

## 📖 Visão Geral

Este projeto implementa o **Design Pattern Builder** para facilitar a criação de objetos complexos (ex: pedidos) de forma fluente e segura.

## 🎯 Problema Resolvido
- **Construtores com muitos parâmetros**
- **Configuração opcional de propriedades**
- **Imutabilidade e validação**

## 🏗️ Estrutura
- `Pedido`: Classe alvo
- `Pedido.Builder`: Classe interna para construção
- Métodos encadeados para configuração
- `Build()`: Valida e retorna o objeto

## 💡 Exemplo de Código

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

## 🔄 Fluxo
1. Instancia o builder
2. Encadeia métodos para configurar
3. Chama `Build()` para criar o objeto

## 📝 Vantagens
- Código mais legível
- Validação centralizada
- Facilidade de extensão

## 🚀 Execução

```bash
cd Builder
 dotnet run
```

## 🔗 Recursos
- [Builder - Refactoring Guru](https://refactoring.guru/design-patterns/builder)
- [Documentação Microsoft](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#builder)
