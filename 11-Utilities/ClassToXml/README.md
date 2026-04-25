# ClassToXml

## Visão geral

Serialize a C# class hierarchy to XML (and back) using `XmlSerializer`.

Highlights:
- `XmlRoot`, `XmlElement`, `XmlAttribute` attributes
- Serialize to string/file and deserialize from string
- Simple namespace demo via `XmlSerializerNamespaces`

Try it:
- Build and run the project. Inspect `invoice.xml` in the output folder.

Extend:
- Add lists (`List<T>`) with `[XmlArray]/[XmlArrayItem]`
- Use `[XmlIgnore]` for fields you don’t want in XML
- Customize element names and ordering
- Add XSD and validate serialized XML

## Conceitos abordados

- Exemplo didático sobre ClassToXml no contexto de utilitários, transformação de dados e observabilidade.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como ClassToXml se aplica em um cenário prático de utilitários, transformação de dados e observabilidade.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
ClassToXml/
+-- ClassToXml.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 11-Utilities/ClassToXml/ClassToXml.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.
