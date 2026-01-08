using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System.IO;
using ExcelCell = NPOI.SS.UserModel.ICell;
using WordCell = NPOI.XWPF.UserModel.ICell;

namespace NPOIDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== NPOI Demo - Gerando arquivos Excel e Word ===\n");

            // Criar arquivo Excel
            CreateExcelFile();

            // Criar arquivo Word com template
            CreateWordDocument();

            Console.WriteLine("\nArquivos gerados com sucesso!");
            Console.WriteLine("Pressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        static void CreateExcelFile()
        {
            Console.WriteLine("Criando arquivo Excel...");

            // Criar workbook (arquivo Excel)
            IWorkbook workbook = new XSSFWorkbook();

            // Criar planilha
            ISheet sheet = workbook.CreateSheet("Relatório de Vendas");

            // Criar estilos
            ICellStyle headerStyle = workbook.CreateCellStyle();
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.FontHeightInPoints = 12;
            headerFont.Color = IndexedColors.White.Index;
            headerStyle.SetFont(headerFont);
            headerStyle.FillForegroundColor = IndexedColors.Blue.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;

            ICellStyle currencyStyle = workbook.CreateCellStyle();
            IDataFormat format = workbook.CreateDataFormat();
            currencyStyle.DataFormat = format.GetFormat("R$ #,##0.00");

            // Criar cabeçalho
            IRow headerRow = sheet.CreateRow(0);
            string[] headers = { "ID", "Produto", "Quantidade", "Preço Unitário", "Total" };

            for (int i = 0; i < headers.Length; i++)
            {
                ExcelCell cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            // Adicionar dados de exemplo
            var salesData = new[]
            {
                new { Id = 1, Product = "Notebook Dell", Quantity = 5, Price = 3500.00 },
                new { Id = 2, Product = "Mouse Logitech", Quantity = 20, Price = 85.50 },
                new { Id = 3, Product = "Teclado Mecânico", Quantity = 15, Price = 450.00 },
                new { Id = 4, Product = "Monitor LG 27\"", Quantity = 8, Price = 1200.00 },
                new { Id = 5, Product = "Webcam Full HD", Quantity = 12, Price = 320.00 }
            };

            int rowNum = 1;
            double grandTotal = 0;

            foreach (var sale in salesData)
            {
                IRow row = sheet.CreateRow(rowNum++);
                double total = sale.Quantity * sale.Price;
                grandTotal += total;

                row.CreateCell(0).SetCellValue(sale.Id);
                row.CreateCell(1).SetCellValue(sale.Product);
                row.CreateCell(2).SetCellValue(sale.Quantity);
                
                ExcelCell priceCell = row.CreateCell(3);
                priceCell.SetCellValue(sale.Price);
                priceCell.CellStyle = currencyStyle;

                ExcelCell totalCell = row.CreateCell(4);
                totalCell.SetCellValue(total);
                totalCell.CellStyle = currencyStyle;
            }

            // Adicionar linha de total
            IRow totalRow = sheet.CreateRow(rowNum);
            ExcelCell totalLabelCell = totalRow.CreateCell(3);
            totalLabelCell.SetCellValue("Total Geral:");
            totalLabelCell.CellStyle = headerStyle;

            ExcelCell grandTotalCell = totalRow.CreateCell(4);
            grandTotalCell.SetCellValue(grandTotal);
            grandTotalCell.CellStyle = currencyStyle;

            // Auto-ajustar colunas
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            // Salvar arquivo
            string excelPath = "RelatorioVendas.xlsx";
            using (FileStream stream = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(stream);
            }

            Console.WriteLine($"✓ Arquivo Excel criado: {Path.GetFullPath(excelPath)}");
        }

        static void CreateWordDocument()
        {
            Console.WriteLine("\nCriando arquivo Word...");

            // Criar documento Word
            XWPFDocument doc = new XWPFDocument();

            // Adicionar título
            XWPFParagraph titleParagraph = doc.CreateParagraph();
            titleParagraph.Alignment = ParagraphAlignment.CENTER;
            XWPFRun titleRun = titleParagraph.CreateRun();
            titleRun.SetText("RELATÓRIO DE VENDAS MENSAL");
            titleRun.IsBold = true;
            titleRun.FontSize = 18;
            titleRun.FontFamily = "Arial";

            // Adicionar espaço
            doc.CreateParagraph();

            // Adicionar informações do cabeçalho
            AddParagraph(doc, "Empresa:", "TechStore LTDA");
            AddParagraph(doc, "Período:", "Janeiro/2026");
            AddParagraph(doc, "Data de Emissão:", DateTime.Now.ToString("dd/MM/yyyy"));

            // Adicionar espaço
            doc.CreateParagraph();

            // Adicionar seção
            XWPFParagraph sectionParagraph = doc.CreateParagraph();
            XWPFRun sectionRun = sectionParagraph.CreateRun();
            sectionRun.SetText("1. RESUMO EXECUTIVO");
            sectionRun.IsBold = true;
            sectionRun.FontSize = 14;

            // Adicionar conteúdo
            XWPFParagraph contentParagraph = doc.CreateParagraph();
            contentParagraph.Alignment = ParagraphAlignment.BOTH;
            XWPFRun contentRun = contentParagraph.CreateRun();
            contentRun.SetText("Este relatório apresenta os resultados das vendas realizadas durante o mês de janeiro de 2026. " +
                "O desempenho geral foi positivo, com destaque para a categoria de notebooks, que apresentou " +
                "crescimento significativo em relação ao mês anterior.");
            contentRun.FontSize = 11;

            // Adicionar espaço
            doc.CreateParagraph();

            // Adicionar tabela
            XWPFTable table = doc.CreateTable(6, 3);
            table.Width = 5000;

            // Cabeçalho da tabela
            XWPFTableRow headerRow = table.GetRow(0);
            SetCellText(headerRow.GetCell(0), "Categoria", true);
            SetCellText(headerRow.GetCell(1), "Quantidade", true);
            SetCellText(headerRow.GetCell(2), "Valor Total", true);

            // Dados da tabela
            string[][] tableData = new string[][]
            {
                new string[] { "Notebooks", "5", "R$ 17.500,00" },
                new string[] { "Periféricos", "35", "R$ 8.767,50" },
                new string[] { "Monitores", "8", "R$ 9.600,00" },
                new string[] { "Webcams", "12", "R$ 3.840,00" },
                new string[] { "Total", "60", "R$ 39.707,50" }
            };

            for (int i = 0; i < tableData.Length; i++)
            {
                XWPFTableRow row = table.GetRow(i + 1);
                for (int j = 0; j < tableData[i].Length; j++)
                {
                    SetCellText(row.GetCell(j), tableData[i][j], i == tableData.Length - 1);
                }
            }

            // Adicionar espaço
            doc.CreateParagraph();

            // Adicionar conclusão
            XWPFParagraph conclusionTitle = doc.CreateParagraph();
            XWPFRun conclusionTitleRun = conclusionTitle.CreateRun();
            conclusionTitleRun.SetText("2. CONCLUSÃO");
            conclusionTitleRun.IsBold = true;
            conclusionTitleRun.FontSize = 14;

            XWPFParagraph conclusionParagraph = doc.CreateParagraph();
            conclusionParagraph.Alignment = ParagraphAlignment.BOTH;
            XWPFRun conclusionRun = conclusionParagraph.CreateRun();
            conclusionRun.SetText("Os resultados demonstram um crescimento consistente nas vendas. " +
                "Recomenda-se manter o foco em produtos de maior margem e expandir o portfólio de periféricos.");
            conclusionRun.FontSize = 11;

            // Adicionar rodapé
            doc.CreateParagraph();
            doc.CreateParagraph();
            XWPFParagraph footerParagraph = doc.CreateParagraph();
            footerParagraph.Alignment = ParagraphAlignment.CENTER;
            XWPFRun footerRun = footerParagraph.CreateRun();
            footerRun.SetText("___________________________________________");
            footerRun.AddBreak();
            footerRun.SetText("Assinatura do Responsável");
            footerRun.FontSize = 10;

            // Salvar documento
            string wordPath = "RelatorioVendas.docx";
            using (FileStream stream = new FileStream(wordPath, FileMode.Create, FileAccess.Write))
            {
                doc.Write(stream);
            }

            Console.WriteLine($"✓ Arquivo Word criado: {Path.GetFullPath(wordPath)}");
        }

        static void AddParagraph(XWPFDocument doc, string label, string value)
        {
            XWPFParagraph paragraph = doc.CreateParagraph();
            
            XWPFRun labelRun = paragraph.CreateRun();
            labelRun.SetText(label + " ");
            labelRun.IsBold = true;
            labelRun.FontSize = 11;

            XWPFRun valueRun = paragraph.CreateRun();
            valueRun.SetText(value);
            valueRun.FontSize = 11;
        }

        static void SetCellText(XWPFTableCell cell, string text, bool isBold = false)
        {
            XWPFParagraph paragraph = cell.Paragraphs[0];
            XWPFRun run = paragraph.CreateRun();
            run.SetText(text);
            run.IsBold = isBold;
            run.FontSize = 10;
        }
    }
}
