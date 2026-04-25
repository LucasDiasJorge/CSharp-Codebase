# Visitor Pattern Example

## Visão geral

Simple and intuitive demo of the Visitor pattern.

Structure:
- `IElement` — interface for elements that accept visitors.
- `IVisitor` — visitor interface with visit methods for each concrete element.
- `Book`, `Dvd` — concrete elements.
- `PriceVisitor` — computes total price.
- `ShippingVisitor` — computes shipping cost.

Run:
```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\DesignPattern\Visitor"
dotnet run
```

This prints a breakdown from both visitors and a total for each.

## Conceitos abordados

- Exemplo didático sobre Visitor Pattern Example no contexto de design patterns, modelagem OO e código limpo.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Visitor Pattern Example se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
Visitor/
+-- Book.cs
+-- Dvd.cs
+-- IElement.cs
+-- IVisitor.cs
+-- PriceVisitor.cs
+-- Program.cs
+-- ShippingVisitor.cs
\-- Visitor.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/Visitor/Visitor.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.
