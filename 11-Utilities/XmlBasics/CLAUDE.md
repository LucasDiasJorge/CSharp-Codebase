# CLAUDE.md — XmlBasics

Console: manipulação de XML com LINQ to XML (`System.Xml.Linq`). Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/XmlBasics/XmlBasics.csproj
dotnet run --project 11-Utilities/XmlBasics/XmlBasics.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): criação de documento com `XDocument`/`XElement`, consulta com LINQ, alteração e remoção de nós.

Distinção em relação a `ClassToXml`: lá o XML é gerado a partir de uma classe (`XmlSerializer`); aqui o documento é construído e consultado como **estrutura de dados**, sem tipo C# correspondente — o caminho para XML de esquema desconhecido.

## Pontos de atenção

- TFM `net9.0`, sem pacotes externos.
- README local em inglês. Mantenha o idioma do arquivo ao editá-lo.
- Em documentos com **namespace**, `Element("nome")` não encontra nada sem o `XNamespace` — é a armadilha mais comum ao estender o exemplo.
