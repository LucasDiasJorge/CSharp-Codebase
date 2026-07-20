using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace HtmlToPdfAndTemplateEngine.Services;

public sealed class PdfConverter
{
    public async Task ConvertHtmlToPdfAsync(string html, string outputPath)
    {
        BrowserFetcher fetcher = new BrowserFetcher();
        await fetcher.DownloadAsync();

        LaunchOptions launchOptions = new LaunchOptions
        {
            Headless = true
        };

        await using IBrowser browser = await Puppeteer.LaunchAsync(launchOptions);
        await using IPage page = await browser.NewPageAsync();
        await page.SetContentAsync(html);

        PdfOptions pdfOptions = new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "20px",
                Bottom = "20px",
                Left = "20px",
                Right = "20px"
            }
        };

        await page.PdfAsync(outputPath, pdfOptions);
    }
}
