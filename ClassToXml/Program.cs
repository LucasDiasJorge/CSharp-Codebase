using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ClassToXml;

// Domain classes with XML attributes
[XmlRoot("Invoice")] 
public class Invoice
{
    [XmlElement("ID")] public int Id { get; set; }
    [XmlElement("Amount")] public decimal Amount { get; set; }
    public Customer Customer { get; set; } = new();

    // Optional attributes example
    [XmlAttribute("currency")] public string Currency { get; set; } = "USD";
}

public class Customer
{
    [XmlElement("Name")] public string Name { get; set; } = string.Empty;
    [XmlElement("Country")] public string Country { get; set; } = string.Empty;
}

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Class -> XML (XmlSerializer)\n");

        var invoice = new Invoice
        {
            Id = 1001,
            Amount = 1499.99m,
            Currency = "BRL",
            Customer = new Customer { Name = "Lucas", Country = "BR" }
        };

        // Serialize to XML string
        var serializer = new XmlSerializer(typeof(Invoice));
        string xml;
        using (var sw = new Utf8StringWriter())
        {
            serializer.Serialize(sw, invoice);
            xml = sw.ToString();
        }
        Console.WriteLine("Serialized XML:\n" + xml + "\n");

        // Deserialize back to object
        Invoice invoice2;
        using (var sr = new StringReader(xml))
        {
            invoice2 = (Invoice)serializer.Deserialize(sr)!;
        }
        Console.WriteLine($"Deserialized -> Id={invoice2.Id}, Amount={invoice2.Amount}, Currency={invoice2.Currency}, Customer={invoice2.Customer.Name}/{invoice2.Customer.Country}");

        // Save to file
        var outPath = Path.Combine(AppContext.BaseDirectory, "invoice.xml");
        File.WriteAllText(outPath, xml, new UTF8Encoding(false));
        Console.WriteLine("Saved to: " + outPath);

        // Namespaces example
        var ns = new XmlSerializerNamespaces();
        ns.Add("ubl", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
        using (var sw = new Utf8StringWriter())
        {
            serializer.Serialize(sw, invoice, ns);
            Console.WriteLine("\nSerialized with namespace prefix (logical; serializer may or may not emit depending on attributes):\n" + sw);
        }
    }

    // Ensure UTF-8 encoding without BOM for serializer output to string
    public sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}
