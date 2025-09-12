# Chain of Responsibility Pattern - Aprovação de Despesas

## 📖 Visão Geral

Este projeto implementa o **Design Pattern Chain of Responsibility** através de um fluxo de aprovação de despesas. O padrão permite que uma solicitação percorra uma cadeia de objetos até que algum deles a trate, promovendo baixo acoplamento e flexibilidade.

## 🎯 Problema Resolvido

- **Evita condicionais aninhadas** para múltiplos níveis de decisão
- **Permite adicionar/remover aprovadores** sem alterar o fluxo principal
- **Facilita extensão** para novos níveis de aprovação

## 🏗️ Estrutura

- `Approver` (abstrato): Define o contrato e referência ao próximo
- `Manager`, `Director`, `CEO`: Implementações concretas
- `SetNext`: Encadeia aprovadores
- `ApproveExpense`: Processa ou delega

## 💡 Exemplo de Código

```csharp
Approver manager = new Manager();
Approver director = new Director();
Approver ceo = new CEO();

manager.SetNext(director);
director.SetNext(ceo);

manager.ApproveExpense(500);    // Manager aprova
manager.ApproveExpense(2000);   // Director aprova
manager.ApproveExpense(15000);  // CEO aprova
manager.ApproveExpense(60000);  // Não aprovado
```

## 🔄 Fluxo

1. O pedido é enviado ao primeiro aprovador
2. Se ele não puder aprovar, delega ao próximo
3. O processo segue até alguém aprovar ou a cadeia acabar

## 📝 Vantagens
- Baixo acoplamento
- Extensível
- Clareza no fluxo de decisão

## 🚀 Execução

```bash
cd ChainOfResponsability
 dotnet run
```

## 🔗 Recursos
- [Chain of Responsibility - Refactoring Guru](https://refactoring.guru/design-patterns/chain-of-responsibility)
- [Documentação Microsoft](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#chain-of-responsibility)
