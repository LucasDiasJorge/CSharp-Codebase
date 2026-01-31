namespace PDFGenerator;

using System;
using System.Data;
using System.IO;
using FastReport;
using FastReport.Export.PdfSimple;

class Program
{
    static void Main()
    {
        Console.WriteLine("Generating sample PDF using FastReport (FRX example)...");

        // build sample data
        var table = new DataTable("MyData");
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(1, "Alice");
        table.Rows.Add(2, "Bob");
        table.Rows.Add(3, "Carlos");

        var reportsDir = Path.Combine(AppContext.BaseDirectory, "Reports");
        var templatePath = Path.Combine(reportsDir, "MyReport.frx");

        // If the FRX template doesn't exist, create and save a simple one programmatically
        if (!File.Exists(templatePath))
        {
            Directory.CreateDirectory(reportsDir);
            using var tmp = new Report();
            tmp.RegisterData(table, "MyData");
            var dsTmp = tmp.GetDataSource("MyData");
            if (dsTmp != null) dsTmp.Enabled = true;

            var pageTmp = new ReportPage();
            tmp.Pages.Add(pageTmp);

            var dataBandTmp = new DataBand();
            dataBandTmp.DataSource = dsTmp;
            pageTmp.Bands.Add(dataBandTmp);

            var txtTmp = new FastReport.TextObject();
            txtTmp.Bounds = new System.Drawing.RectangleF(0, 0, 500, 40);
            txtTmp.Text = "[MyData.Id] - [MyData.Name]";
            dataBandTmp.Objects.Add(txtTmp);

            // save the FRX template to disk
            tmp.Save(templatePath);
            Console.WriteLine($"FRX template created at: {templatePath}");
        }

        // Load the FRX template from disk and export using registered data
        using var report = new Report();
        report.Load(templatePath);

        // register data (FRX may reference data by name)
        report.RegisterData(table, "MyData");
        var ds = report.GetDataSource("MyData");
        if (ds != null) ds.Enabled = true;

        report.Prepare();
        var outPath = Path.Combine(AppContext.BaseDirectory, "report-from-frx.pdf");
        using var pdfExport = new PDFSimpleExport();
        report.Export(pdfExport, outPath);

        Console.WriteLine($"PDF generated from FRX: {outPath}");
    }
}
