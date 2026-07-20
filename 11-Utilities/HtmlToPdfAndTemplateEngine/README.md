# HtmlToPdfAndTemplateEngine

Geração de PDF a partir de um template HTML preenchido com dados dinâmicos em tempo de execução.

## Visão geral

O exemplo simula a emissão de uma fatura: um modelo de domínio (`InvoiceData`/`InvoiceItem`) é projetado em um objeto de visualização com valores já formatados e injetado em um template HTML usando o motor de templates Scriban. O HTML resultante é então convertido em PDF por um navegador headless (Chromium) controlado via PuppeteerSharp. O fluxo evidencia a separação entre dados, apresentação (template) e renderização (conversão para PDF).

## Conceitos abordados

- Template engine (Scriban) para separar dados de marcação HTML.
- Projeção de modelo de domínio em modelo de apresentação (view model).
- Conversão de HTML para PDF com navegador headless (PuppeteerSharp/Chromium).
- Tratamento de indisponibilidade de dependência externa em tempo de execução.

## Objetivos de aprendizagem

- Renderizar templates HTML dinamicamente a partir de objetos C#.
- Gerar documentos PDF fiéis ao HTML/CSS de origem.
- Separar responsabilidades entre modelo de domínio, view model e renderização.
- Lidar com falhas de dependências externas (download do Chromium) de forma resiliente.

## Estrutura do projeto

```text
HtmlToPdfAndTemplateEngine/
|-- Models/
|   |-- InvoiceData.cs
|   `-- InvoiceItem.cs
|-- Services/
|   |-- HtmlTemplateRenderer.cs
|   `-- PdfConverter.cs
|-- Templates/
|   `-- invoice-template.html
|-- Program.cs
`-- HtmlToPdfAndTemplateEngine.csproj
```

## Como executar

```bash
dotnet run --project 11-Utilities/HtmlToPdfAndTemplateEngine/HtmlToPdfAndTemplateEngine.csproj
```

Na primeira execução, o PuppeteerSharp baixa automaticamente um Chromium compatível para o cache local do usuário. O PDF gerado fica em `Output/invoice.pdf`, relativo à raiz do projeto.

## Boas práticas e pontos de atenção

- O template usa a convenção padrão do Scriban (snake_case) para acessar propriedades do view model (ex.: `{{ invoice_number }}`).
- Valores monetários e datas são formatados no C# antes de chegar ao template, mantendo o HTML livre de lógica de formatação.
- Ambientes sem acesso à internet ou sem permissão para baixar o Chromium não conseguem gerar o PDF; nesse caso, o programa cai para um fallback e salva o HTML renderizado em `Output/invoice.html`.
- O download do Chromium ocorre em tempo de execução, não em tempo de build — o `dotnet build` não depende dele.

## Conteúdo complementar

### Saída esperada

```text
Template renderizado a partir de dados dinamicos.
Gerando PDF em: .../Output/invoice.pdf
PDF gerado com sucesso.
```

### Dependências

| Pacote | Papel |
|--------|-------|
| `Scriban` | Motor de templates para gerar o HTML a partir do view model |
| `PuppeteerSharp` | Controla um Chromium headless para converter HTML em PDF |

## Referências e documentação complementar

- [Scriban](https://github.com/scriban/scriban)
- [PuppeteerSharp](https://www.puppeteersharp.com/)
