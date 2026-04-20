# NPOIDemo - Geração de Arquivos Excel e Word

## Visão geral

Este projeto demonstra o uso da biblioteca NPOI para criar arquivos Excel (.xlsx) e Word (.docx) em C# sem necessidade de ter o Microsoft Office instalado.

## Conceitos abordados

- Exemplo didático sobre NPOIDemo - Geração de Arquivos Excel e Word no contexto de utilitários, transformação de dados e observabilidade.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como NPOIDemo - Geração de Arquivos Excel e Word se aplica em um cenário prático de utilitários, transformação de dados e observabilidade.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
NPOIDemo/
+-- NPOIDemo/
+-- NPOIDemo.csproj
+-- Program.cs
+-- RelatorioVendas.docx
\-- RelatorioVendas.xlsx
```

## Como executar

```bash
dotnet run --project 11-Utilities/NPOIDemo/NPOIDemo.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Tecnologias

- **.NET 9.0** (ou versão configurada)
- **NPOI 2.7.5** - Biblioteca para manipulação de arquivos Office

##### 1. Geração de Arquivo Excel (.xlsx)

- Criação de planilha com dados formatados
- Estilos personalizados (cabeçalho, cores, fontes)
- Formatação de valores monetários
- Auto-ajuste de colunas
- Cálculo de totais

**Exemplo de dados gerados:**
- Relatório de vendas com produtos
- Colunas: ID, Produto, Quantidade, Preço Unitário, Total
- Linha de total geral

##### 2. Geração de Arquivo Word (.docx)

- Documento com template estruturado
- Título formatado e centralizado
- Parágrafos com alinhamento justificado
- Tabela de dados formatada
- Seções organizadas (Resumo, Conclusão)
- Assinatura ao final

##### Pré-requisitos

- .NET SDK instalado (versão 6.0 ou superior)

##### Arquivos Gerados

Após executar o projeto, serão criados dois arquivos na pasta raiz:

1. **RelatorioVendas.xlsx** - Planilha Excel com dados de vendas
2. **RelatorioVendas.docx** - Documento Word com relatório completo

##### Program.cs

- `Main()` - Método principal que coordena a geração dos arquivos
- `CreateExcelFile()` - Cria o arquivo Excel com dados e formatação
- `CreateWordDocument()` - Cria o documento Word com template
- `AddParagraph()` - Método auxiliar para adicionar parágrafos formatados
- `SetCellText()` - Método auxiliar para formatar células de tabela

##### Excel (NPOI.XSSF)

- `XSSFWorkbook` - Workbook para formato .xlsx
- `ISheet` - Planilha
- `IRow` e `ICell` - Linhas e células
- `ICellStyle` - Estilos de célula
- `IFont` - Formatação de fontes
- Formatação de valores monetários

##### Word (NPOI.XWPF)

- `XWPFDocument` - Documento Word
- `XWPFParagraph` - Parágrafos
- `XWPFRun` - Trechos de texto formatado
- `XWPFTable` - Tabelas
- Alinhamento e estilos de texto

##### Exemplo de Uso

```csharp
// Criar arquivo Excel
IWorkbook workbook = new XSSFWorkbook();
ISheet sheet = workbook.CreateSheet("MinhaPlanilha");
IRow row = sheet.CreateRow(0);
row.CreateCell(0).SetCellValue("Olá NPOI!");

using (FileStream stream = new FileStream("exemplo.xlsx", FileMode.Create))
{
    workbook.Write(stream);
}

// Criar arquivo Word
XWPFDocument doc = new XWPFDocument();
XWPFParagraph paragraph = doc.CreateParagraph();
XWPFRun run = paragraph.CreateRun();
run.SetText("Olá NPOI!");

using (FileStream stream = new FileStream("exemplo.docx", FileMode.Create))
{
    doc.Write(stream);
}
```

##### Vantagens do NPOI

- ✅ Não requer Microsoft Office instalado
- ✅ Código multiplataforma (Windows, Linux, macOS)
- ✅ Alto desempenho
- ✅ Suporte para formatos .xls, .xlsx, .doc, .docx
- ✅ API intuitiva e bem documentada

##### Observações

- Os arquivos são gerados na pasta raiz do projeto
- Certifique-se de ter permissões de escrita na pasta
- Os estilos e formatações podem ser personalizados conforme necessidade

## Referências

- **Documentação NPOI**: [https://github.com/nissl-lab/npoi](https://github.com/nissl-lab/npoi)
- **Exemplos**: [https://github.com/nissl-lab/npoi/wiki](https://github.com/nissl-lab/npoi/wiki)
