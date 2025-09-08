using System;
using System.Linq;
using System.Xml.Linq;

namespace XmlBasics;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("XML Basics with LINQ to XML (System.Xml.Linq)\n");

        // 1) Load XML from string
        string xmlString = @"<Invoice><ID>123</ID><Amount>500</Amount><Customer><Name>Lucas</Name><Country>BR</Country></Customer></Invoice>";
        XDocument docFromString = XDocument.Parse(xmlString);
        Console.WriteLine("Loaded from string: ID=" + docFromString.Root!.Element("ID")!.Value);

        // 2) Load XML from file (sample located in Data/invoice.xml)
        var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "invoice.xml");
        XDocument docFromFile = XDocument.Load(filePath);
        Console.WriteLine("Loaded from file: Amount=" + docFromFile.Root!.Element("Amount")!.Value);

        // 3) Read nested elements
        var name = docFromFile.Root.Element("Customer")!.Element("Name")!.Value;
        Console.WriteLine("Customer name=" + name);

        // 4) LINQ query: list Customers in the document
        var customers = from c in docFromFile.Descendants("Customer")
                        select new
                        {
                            Name = (string?)c.Element("Name"),
                            Country = (string?)c.Element("Country")
                        };

        foreach (var c in customers)
            Console.WriteLine($"- {c.Name} ({c.Country})");

        // 5) Create new document dynamically
        XDocument newDoc = new(
            new XElement("Invoice",
                new XElement("ID", 456),
                new XElement("Amount", 999),
                new XElement("Customer",
                    new XElement("Name", "Maria"),
                    new XElement("Country", "PT")
                )
            )
        );
        Console.WriteLine("\nNew document: \n" + newDoc);

        // 6) Update value
        docFromFile.Root.Element("Amount")!.Value = "1500";
        Console.WriteLine("Updated Amount to 1500");

        // 7) Add element
        docFromFile.Root.Add(new XElement("Status", "Paid"));
        Console.WriteLine("Added <Status>Paid</Status>");

        // 8) Remove element
        docFromFile.Root.Element("Status")!.Remove();
        Console.WriteLine("Removed <Status>");

        // 9) Save to file
        var outPath = Path.Combine(AppContext.BaseDirectory, "invoice_updated.xml");
        docFromFile.Save(outPath);
        Console.WriteLine("Saved to: " + outPath);

        // 10) Namespaces example (load namespaced XML)
        var nsPath = Path.Combine(AppContext.BaseDirectory, "Data", "invoice.namespaced.xml");
        var nsDoc = XDocument.Load(nsPath);
        XNamespace ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        var nsId = nsDoc.Root!.Element(ns + "ID")!.Value;
        Console.WriteLine("Namespaced ID=" + nsId);

        Console.WriteLine("\nDone.");
    }
}
