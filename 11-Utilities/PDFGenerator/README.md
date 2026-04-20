# PDFGenerator

## Visão geral

Geração de documentos PDF usando FastReport Open Source com templates FRX e dados dinâmicos.

## Conceitos abordados

- **FastReport**: Biblioteca open source para geração de relatórios
- **Templates FRX**: Arquivos de template de relatório reutilizáveis
- **DataTable**: Fonte de dados para relatórios
- **PDF Export**: Exportação de relatórios para PDF

## Objetivos de aprendizagem

- Gerar PDFs programaticamente em .NET
- Criar e usar templates de relatório FRX
- Vincular dados dinâmicos a relatórios
- Exportar relatórios para diferentes formatos

## Estrutura do projeto

```text
PDFGenerator/
+-- Reports/
|   \-- MyReport.frx
+-- PDFGenerator.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 11-Utilities/PDFGenerator/PDFGenerator.csproj
```

## Boas práticas e pontos de atenção

- Separar templates em arquivos FRX para reuso
- Usar DataTable ou objetos fortemente tipados como fonte
- Validar dados antes de gerar relatório
- Implementar tratamento de erros na geração

### Pontos de Atenção

- FastReport.OpenSource tem recursos limitados vs versão comercial
- Fontes devem estar disponíveis no sistema
- Para relatórios complexos, considerar designer visual

## Conteúdo complementar

##### Estrutura do Projeto

```
PDFGenerator/
├── Reports/
│   └── MyReport.frx          # Template de relatório
├── Program.cs
├── PDFGenerator.csproj
└── README.md
```

##### Saída Esperada

```
Generating sample PDF using FastReport (FRX example)...
FRX template created at: .../Reports/MyReport.frx
PDF generated from FRX: .../report-from-frx.pdf
```

##### Criando Dados de Exemplo

```csharp
DataTable table = new DataTable("MyData");
table.Columns.Add("Id", typeof(int));
table.Columns.Add("Name", typeof(string));
table.Rows.Add(1, "Alice");
table.Rows.Add(2, "Bob");
table.Rows.Add(3, "Carlos");
```

##### Criando Template Programaticamente

```csharp
using Report tmp = new Report();
tmp.RegisterData(table, "MyData");

DataSourceBase dsTmp = tmp.GetDataSource("MyData");
if (dsTmp != null) dsTmp.Enabled = true;

ReportPage pageTmp = new ReportPage();
tmp.Pages.Add(pageTmp);

DataBand dataBandTmp = new DataBand();
dataBandTmp.DataSource = dsTmp;
pageTmp.Bands.Add(dataBandTmp);

TextObject txtTmp = new TextObject();
txtTmp.Bounds = new RectangleF(0, 0, 500, 40);
txtTmp.Text = "[MyData.Id] - [MyData.Name]";
dataBandTmp.Objects.Add(txtTmp);

// Salvar template
tmp.Save(templatePath);
```

##### Carregando Template e Exportando PDF

```csharp
using Report report = new Report();
report.Load(templatePath);

// Registrar dados
report.RegisterData(table, "MyData");
DataSourceBase ds = report.GetDataSource("MyData");
if (ds != null) ds.Enabled = true;

// Preparar e exportar
report.Prepare();

using PDFSimpleExport pdfExport = new PDFSimpleExport();
report.Export(pdfExport, "output.pdf");
```

##### Dependências

| Pacote | Descrição |
|--------|-----------|
| `FastReport.OpenSource` | Engine de relatórios |
| `FastReport.OpenSource.Export.PdfSimple` | Exportação PDF |

##### Instalação

```bash
dotnet add package FastReport.OpenSource
dotnet add package FastReport.OpenSource.Export.PdfSimple
```

##### Estrutura do Template FRX

O arquivo FRX é XML que define:

- **ReportPage**: Páginas do relatório
- **DataBand**: Banda que repete para cada registro
- **TextObject**: Campos de texto com expressões
- **Expressions**: `[DataSource.Campo]` para dados dinâmicos

## Referências

- [FastReport Open Source](https://github.com/FastReports/FastReport)
- [FastReport Documentation](https://www.fast-report.com/en/documentation/)
- [PDF Generation in .NET](https://learn.microsoft.com/en-us/dotnet/standard/io/)
