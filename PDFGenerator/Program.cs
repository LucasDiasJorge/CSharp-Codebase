namespace PDFGenerator;

using System;
using System.Data;
using FastReport;
using FastReport.Export.PdfSimple;

class Program
{
    static void Main()
    {
        Console.WriteLine("Generating sample PDF using FastReport...");

        // build sample data
        var table = new DataTable("MyData");
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Alice");
        table.Rows.Add(2, "Bob");
        table.Rows.Add(3, "Carlos");

        using var report = new Report();

        // register data and enable it
        report.RegisterData(table, "MyData");
        var ds = report.GetDataSource("MyData");
        if (ds != null) ds.Enabled = true;

        // build a very small report layout in code
        var page = new ReportPage();
        report.Pages.Add(page);

        var dataBand = new DataBand();
        dataBand.DataSource = ds;
        page.Bands.Add(dataBand);

        var txt = new FastReport.TextObject();
        txt.Bounds = new System.Drawing.RectangleF(0, 0, 500, 40);
        txt.Text = "[MyData.Id] - [MyData.Name]";
        dataBand.Objects.Add(txt);

        // prepare and export
        report.Prepare();
        var outPath = "report-output.pdf";
        using var pdfExport = new PDFSimpleExport();
        report.Export(pdfExport, outPath);

        Console.WriteLine($"PDF generated: {outPath}");
    }
}
