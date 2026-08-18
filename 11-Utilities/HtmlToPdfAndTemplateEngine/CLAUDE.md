# CLAUDE.md — HtmlToPdfAndTemplateEngine

Console: fatura gerada a partir de template HTML e convertida em PDF por navegador headless. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 11-Utilities/HtmlToPdfAndTemplateEngine/HtmlToPdfAndTemplateEngine.csproj
dotnet run --project 11-Utilities/HtmlToPdfAndTemplateEngine/HtmlToPdfAndTemplateEngine.csproj
```

## Estrutura interna

Pipeline de três estágios, deliberadamente separados:

1. `Models/InvoiceData.cs` + `InvoiceItem.cs` — **dados** do domínio.
2. `Services/HtmlTemplateRenderer.cs` — **apresentação**: injeta os dados em `Templates/invoice-template.html` usando o motor Scriban. Valores já chegam formatados, o template não faz cálculo.
3. `Services/PdfConverter.cs` — **renderização**: PuppeteerSharp dirige um Chromium headless que imprime o HTML em PDF.

## Pontos de atenção

- **Primeira execução baixa o Chromium** (centenas de MB) via PuppeteerSharp, e exige internet. Execuções seguintes usam o cache local. Isso torna o primeiro `dotnet run` demorado — não é travamento.
- Gera arquivo PDF no diretório de saída; não comite o artefato.
- TFM **`net10.0`**. `PuppeteerSharp` 20.1.3, `Scriban` 7.2.5.
- Alternativa sem navegador na mesma trilha: `PDFGenerator` (FastReport).
