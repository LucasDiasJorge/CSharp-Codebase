using System;

namespace SOLIDExamples.SRP
{
    public static class SrpExample
    {
        public static void Run()
        {
            Console.WriteLine("Exemplo INCORRETO:");
            var badInvoice = new BadInvoice();
            badInvoice.Process();

            Console.WriteLine("\nExemplo CORRETO:");
            var invoice = new Invoice();
            invoice.CalculateTotal();
            var repo = new InvoiceRepository();
            repo.Save(invoice);
            var printer = new InvoicePrinter();
            printer.Print(invoice);
        }
    }

    // Violando o SRP
    public class BadInvoice
    {
        public void Process()
        {
            Console.WriteLine("Calculando total...");
            Console.WriteLine("Salvando no banco...");
            Console.WriteLine("Imprimindo fatura...");
        }
    }

    // Correto: cada classe tem uma responsabilidade
    public class Invoice
    {
        public void CalculateTotal() => Console.WriteLine("Calculando total...");
    }
    public class InvoiceRepository
    {
        public void Save(Invoice invoice) => Console.WriteLine("Salvando no banco...");
    }
    public class InvoicePrinter
    {
        public void Print(Invoice invoice) => Console.WriteLine("Imprimindo fatura...");
    }
}
