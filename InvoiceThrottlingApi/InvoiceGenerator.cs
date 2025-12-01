namespace InvoiceThrottlingApi.Services;

using InvoiceThrottlingApi.Models;

public interface IInvoiceGenerator
{
    List<Invoice> GenerateInvoices(int count);
}

public class InvoiceGenerator : IInvoiceGenerator
{
    private readonly Random _random = new();
    private readonly string[] _products = ["Notebook", "Mouse", "Teclado", "Monitor", "Headset", "Webcam", "SSD", "Memória RAM"];
    private readonly string[] _customerNames = ["João Silva", "Maria Santos", "Pedro Oliveira", "Ana Costa", "Carlos Souza"];

    public List<Invoice> GenerateInvoices(int count)
    {
        var invoices = new List<Invoice>();
        var baseDate = DateTime.Now.AddDays(-30);

        for (int i = 1; i <= count; i++)
        {
            var items = GenerateItems();
            var totalAmount = items.Sum(item => item.TotalPrice);

            var invoice = new Invoice(
                Id: Guid.NewGuid().ToString(),
                Number: $"NF-{i:D6}",
                IssueDate: baseDate.AddHours(_random.Next(0, 720)),
                TotalAmount: totalAmount,
                CustomerName: _customerNames[_random.Next(_customerNames.Length)],
                CustomerDocument: GenerateDocument(),
                Items: items,
                Status: InvoiceStatus.Pending
            );

            invoices.Add(invoice);
        }

        return invoices;
    }

    private List<InvoiceItem> GenerateItems()
    {
        var itemCount = _random.Next(1, 6);
        var items = new List<InvoiceItem>();

        for (int i = 0; i < itemCount; i++)
        {
            var product = _products[_random.Next(_products.Length)];
            var quantity = _random.Next(1, 11);
            var unitPrice = _random.Next(50, 5000);
            var totalPrice = quantity * unitPrice;

            items.Add(new InvoiceItem(
                ProductCode: $"PROD{_random.Next(1000, 9999)}",
                Description: product,
                Quantity: quantity,
                UnitPrice: unitPrice,
                TotalPrice: totalPrice
            ));
        }

        return items;
    }

    private string GenerateDocument()
    {
        var doc = "";
        for (int i = 0; i < 11; i++)
        {
            doc += _random.Next(0, 10);
        }
        return doc;
    }
}
