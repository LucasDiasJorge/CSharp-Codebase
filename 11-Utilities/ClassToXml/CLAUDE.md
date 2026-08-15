# CLAUDE.md — ClassToXml

Console: serialização de hierarquia de classes para XML e de volta, com `XmlSerializer`. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/ClassToXml/ClassToXml.csproj
dotnet run --project 11-Utilities/ClassToXml/ClassToXml.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): ida e volta completa (objeto → XML → objeto), com atributos de controle de nome, elemento versus atributo e coleções.

Restrições do `XmlSerializer` que o exemplo esbarra e vale preservar: exige construtor público sem parâmetros e só serializa membros públicos — diferente de `System.Text.Json`.

## Pontos de atenção

- TFM `net9.0`, sem pacotes externos.
- README local em inglês. Mantenha o idioma do arquivo ao editá-lo.
- Companheiro de `11-Utilities/XmlBasics`, que cobre a leitura/manipulação com LINQ to XML em vez de serialização.
