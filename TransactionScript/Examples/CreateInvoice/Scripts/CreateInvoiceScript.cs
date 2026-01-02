using TransactionScript.Core;
using TransactionScript.Examples.CreateInvoice.DTOs;

namespace TransactionScript.Examples.CreateInvoice.Scripts;

/// <summary>
/// Script de transação para criação de invoice/nota fiscal
/// 
/// Demonstra cálculos de negócio dentro do script:
/// - Cálculo de totais
/// - Aplicação de impostos
/// - Geração de número sequencial
/// </summary>
public class CreateInvoiceScript : ITransactionScript<CreateInvoiceInput, CreateInvoiceOutput>
{
    private readonly IDataGateway _db;

    public CreateInvoiceScript(IDataGateway db)
    {
        _db = db;
    }

    public ScriptResult<CreateInvoiceOutput> Execute(CreateInvoiceInput input)
    {
        Console.WriteLine("\n[CreateInvoiceScript] Criando invoice...");

        // ========== VALIDAÇÃO ==========
        if (input.CustomerId == Guid.Empty)
        {
            return ScriptResult<CreateInvoiceOutput>.Fail("CustomerId é obrigatório");
        }

        if (string.IsNullOrWhiteSpace(input.CustomerName))
        {
            return ScriptResult<CreateInvoiceOutput>.Fail("Nome do cliente é obrigatório");
        }

        if (input.Items == null || input.Items.Count == 0)
        {
            return ScriptResult<CreateInvoiceOutput>.Fail("Invoice deve ter pelo menos um item");
        }

        if (input.TaxRate < 0 || input.TaxRate > 1)
        {
            return ScriptResult<CreateInvoiceOutput>.Fail("Taxa de imposto deve estar entre 0 e 1");
        }

        // ========== GERAR NÚMERO DA INVOICE ==========
        var invoiceNumber = _db.GetNextInvoiceNumber();
        Console.WriteLine($"[CreateInvoiceScript] Número gerado: {invoiceNumber}");

        // ========== CALCULAR ITENS ==========
        var invoiceItems = new List<InvoiceItemData>();
        decimal subTotal = 0;

        foreach (var item in input.Items)
        {
            if (item.Quantity <= 0)
            {
                return ScriptResult<CreateInvoiceOutput>.Fail($"Quantidade inválida para item: {item.Description}");
            }

            if (item.UnitPrice < 0)
            {
                return ScriptResult<CreateInvoiceOutput>.Fail($"Preço inválido para item: {item.Description}");
            }

            var itemTotal = item.Quantity * item.UnitPrice;
            subTotal += itemTotal;

            invoiceItems.Add(new InvoiceItemData
            {
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = itemTotal
            });

            Console.WriteLine($"    Item: {item.Description} | {item.Quantity} x {item.UnitPrice:C} = {itemTotal:C}");
        }

        // ========== CALCULAR IMPOSTOS ==========
        var tax = subTotal * input.TaxRate;
        var total = subTotal + tax;

        Console.WriteLine($"[CreateInvoiceScript] SubTotal: {subTotal:C}");
        Console.WriteLine($"[CreateInvoiceScript] Imposto ({input.TaxRate:P0}): {tax:C}");
        Console.WriteLine($"[CreateInvoiceScript] Total: {total:C}");

        // ========== CRIAR INVOICE ==========
        var issueDate = DateTime.UtcNow;
        var dueDate = issueDate.AddDays(input.DueDays);

        var invoice = new InvoiceData
        {
            Id = Guid.NewGuid(),
            Number = invoiceNumber,
            CustomerId = input.CustomerId,
            CustomerName = input.CustomerName,
            IssueDate = issueDate,
            DueDate = dueDate,
            Items = invoiceItems,
            SubTotal = subTotal,
            Tax = tax,
            Total = total,
            Status = "Pending"
        };

        _db.SaveInvoice(invoice);

        // ========== RETORNAR RESULTADO ==========
        var output = new CreateInvoiceOutput
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoiceNumber,
            SubTotal = subTotal,
            Tax = tax,
            Total = total,
            DueDate = dueDate
        };

        Console.WriteLine($"[CreateInvoiceScript] ✓ Invoice #{invoiceNumber} criada com sucesso");

        return ScriptResult<CreateInvoiceOutput>.Ok(output);
    }
}
