# XML Basics (LINQ to XML)

## Visão geral

This mini-project teaches XML manipulation in C# using System.Xml.Linq.

It covers:
- Structure of XML and formatting rules
- Loading from string and file
- Reading elements and nested nodes
- LINQ queries with Descendants and projections
- Creating, modifying, adding, and removing elements
- Saving back to file or string
- Namespaces (XNamespace)

Run it and inspect the output to follow along with each step.

How to run:
1) Build the solution.
2) Run the XmlBasics project.

The project copies files under `Data/` to the output folder for easy loading.

Further exercises:
- Add validation with XSD (XmlSchemaSet) and validate documents
- Write a function that converts the Invoice to a DTO and back
- Extend the sample with multiple invoices and filter using LINQ
- Integrate into an ASP.NET endpoint that returns the XML

## Conceitos abordados

- Exemplo didático sobre XML Basics (LINQ to XML) no contexto de utilitários, transformação de dados e observabilidade.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como XML Basics (LINQ to XML) se aplica em um cenário prático de utilitários, transformação de dados e observabilidade.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
XmlBasics/
+-- Data/
|   +-- invoice.namespaced.xml
|   \-- invoice.xml
+-- Program.cs
\-- XmlBasics.csproj
```

## Como executar

```bash
dotnet run --project 11-Utilities/XmlBasics/XmlBasics.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.
