# StrategyIntegration

## Visão geral

Exemplo prático do padrão Strategy aplicado a integrações de dados com diferentes provedores.

## Conceitos abordados

- **Strategy Pattern**: Encapsulamento de algoritmos intercambiáveis em classes separadas
- **Interface Segregation**: Definição de contrato comum para estratégias
- **Dependency Injection**: Injeção de estratégias via construtor
- **Open/Closed Principle**: Extensibilidade sem modificação do código existente

## Objetivos de aprendizagem

- Entender quando e por que usar o padrão Strategy
- Implementar estratégias intercambiáveis em tempo de execução
- Aplicar o princípio Open/Closed em integrações
- Desacoplar lógica de negócio de implementações específicas

## Estrutura do projeto

```text
StrategyIntegration/
+-- IntegrationClasses/
|   +-- FirstIntegration.cs
|   \-- SecondIntegration.cs
+-- Interfaces/
|   \-- IIntegrationStrategy.cs
+-- IntegrationStrategy.cs
+-- Program.cs
+-- Response.cs
\-- StrategyIntegration.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/StrategyIntegration/StrategyIntegration.csproj
```

## Boas práticas e pontos de atenção

- Definir interface clara e coesa para as estratégias
- Usar injeção de dependência para flexibilidade
- Nomear estratégias de forma descritiva
- Considerar Factory para criação de estratégias

### Pontos de Atenção

- Evitar estratégias com responsabilidades muito diferentes
- Cuidado com estado compartilhado entre estratégias
- Em cenários complexos, considerar combinar com Factory Pattern

## Conteúdo complementar

##### Estrutura do Projeto

```
StrategyIntegration/
├── Interfaces/
│   └── IIntegrationStrategy.cs   # Contrato das estratégias
├── IntegrationClasses/
│   ├── FirstIntegration.cs       # Estratégia 1
│   └── SecondIntegration.cs      # Estratégia 2
├── IntegrationStrategy.cs        # Context (executor)
├── Response.cs                   # Modelo de resposta
├── Program.cs
└── README.md
```

##### Saída Esperada

```
FirstIntegration: Processing data integration...
Integrating data from source: { key1: value1 } to destination: Destination1
Response Id: 1, Message: Data integration successful., At: 2025-02-27 10:30:00

SecondIntegration: Processing data integration...
Integrating data from source: { key2: value2 } to destination: Destination2
Response Id: 2, Message: Data integration successful., At: 2025-02-27 10:30:01
```

##### Interface da Estratégia

```csharp
public interface IIntegrationStrategy
{
    Response IntegrateData(Dictionary<string, object> source, string destination);
}
```

##### Implementação de uma Estratégia

```csharp
public class FirstIntegration : IIntegrationStrategy
{
    public Response IntegrateData(Dictionary<string, object> source, string destination)
    {
        Console.WriteLine("FirstIntegration: Processing data integration...");

        string formattedSource = string.Join(", ", source.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        Console.WriteLine($"Integrating data from source: {{ {formattedSource} }} to destination: {destination}");
        
        Response response = new Response(1, "Data integration successful.");
        return response;
    }
}
```

##### Context (Executor)

```csharp
public class IntegrationStrategy
{
    private IIntegrationStrategy _strategy;

    public IntegrationStrategy(IIntegrationStrategy strategy)
    {
        _strategy = strategy;
    }

    public void SetStrategy(IIntegrationStrategy strategy)
    {
        _strategy = strategy;
    }

    public Response ExecuteIntegration(Dictionary<string, object> source, string destination)
    {
        return _strategy.IntegrateData(source, destination);
    }
}
```

##### Uso no Main

```csharp
IntegrationStrategy integrationStrategy = new IntegrationStrategy(new FirstIntegration());
PrintResponse(integrationStrategy.ExecuteIntegration(data1, "Destination1"));

// Troca de estratégia em runtime
integrationStrategy.SetStrategy(new SecondIntegration());
PrintResponse(integrationStrategy.ExecuteIntegration(data2, "Destination2"));
```

## Referências

- [Strategy Pattern - Refactoring Guru](https://refactoring.guru/design-patterns/strategy)
- [Design Patterns in C#](https://learn.microsoft.com/en-us/dotnet/standard/design-patterns/)
