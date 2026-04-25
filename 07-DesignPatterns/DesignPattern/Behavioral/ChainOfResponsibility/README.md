# Chain of Responsibility Pattern - Aprovação de Despesas

## Visão geral

Este projeto implementa o **Design Pattern Chain of Responsibility** através de um fluxo de aprovação de despesas. O padrão permite que uma solicitação percorra uma cadeia de objetos até que algum deles a trate, promovendo baixo acoplamento e flexibilidade.

## Conceitos abordados

- **Evita condicionais aninhadas** para múltiplos níveis de decisão
- **Permite adicionar/remover aprovadores** sem alterar o fluxo principal
- **Facilita extensão** para novos níveis de aprovação

## Objetivos de aprendizagem

- Entender como Chain of Responsibility Pattern - Aprovação de Despesas se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
ChainOfResponsibility/
+-- ChainOfResponsability.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/ChainOfResponsibility/ChainOfResponsability.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Estrutura

- `Approver` (abstrato): Define o contrato e referência ao próximo
- `Manager`, `Director`, `CEO`: Implementações concretas
- `SetNext`: Encadeia aprovadores
- `ApproveExpense`: Processa ou delega

##### Exemplo de Código

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

##### Fluxo

1. O pedido é enviado ao primeiro aprovador
2. Se ele não puder aprovar, delega ao próximo
3. O processo segue até alguém aprovar ou a cadeia acabar

##### Vantagens

- Baixo acoplamento
- Extensível
- Clareza no fluxo de decisão

## Referências

- [Chain of Responsibility - Refactoring Guru](https://refactoring.guru/design-patterns/chain-of-responsibility)
- [Documentação Microsoft](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#chain-of-responsibility)
