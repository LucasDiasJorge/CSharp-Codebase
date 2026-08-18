# CLAUDE.md — PDFGenerator

Console: PDF gerado a partir de template FRX com FastReport. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/PDFGenerator/PDFGenerator.csproj
dotnet run --project 11-Utilities/PDFGenerator/PDFGenerator.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): carrega o relatório, liga a fonte de dados e exporta em PDF. O template `.frx` é o artefato de layout — abordagem de **ferramenta de relatório**, diferente de `HtmlToPdfAndTemplateEngine`, que usa HTML e navegador headless. Vale conhecer as duas: a escolha entre elas depende de quem mantém o layout.

## Pontos de atenção

- **A versão do pacote no `.csproj` está malformada**: `FastReport.OpenSource` aparece com `Version="2026.1.3 2026.1.3"` (valor duplicado com espaço). Se o restore falhar neste projeto, comece por aí.
- TFM **`net10.0`**. Exporta via `FastReport.OpenSource.Export.PdfSimple`.
- Gera PDF no diretório de saída; não comite o artefato.
