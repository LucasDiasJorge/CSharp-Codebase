using TransactionScript.Core;
using TransactionScript.Examples.TransferMoney.DTOs;
using TransactionScript.Examples.TransferMoney.Scripts;
using TransactionScript.Examples.CreateInvoice.DTOs;
using TransactionScript.Examples.CreateInvoice.Scripts;
using TransactionScript.Examples.ProcessRefund.DTOs;
using TransactionScript.Examples.ProcessRefund.Scripts;

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║         Transaction Script Pattern - Demonstração            ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

// Gateway compartilhado (simula banco de dados)
var db = new InMemoryDataGateway();

// IDs das contas de teste
var joaoAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var mariaAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
var notebookProductId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

// ==========================================================
// EXEMPLO 1: Transfer Money Script
// ==========================================================
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("EXEMPLO 1: Transfer Money Transaction Script");
Console.WriteLine(new string('=', 60));

var transferScript = new TransferMoneyScript(db);

// Cenário 1.1: Transferência bem-sucedida
Console.WriteLine("\n--- Cenário 1.1: Transferência bem-sucedida ---");
var transferInput = new TransferMoneyInput
{
    FromAccountId = joaoAccountId,
    ToAccountId = mariaAccountId,
    Amount = 500.00m,
    Description = "Pagamento de aluguel"
};

var transferResult = transferScript.Execute(transferInput);
if (transferResult.Success)
{
    Console.WriteLine($"\n✓ Sucesso!");
    Console.WriteLine($"  Transaction ID: {transferResult.Data!.TransactionId}");
    Console.WriteLine($"  Novo saldo João: {transferResult.Data.FromAccountNewBalance:C}");
    Console.WriteLine($"  Novo saldo Maria: {transferResult.Data.ToAccountNewBalance:C}");
}

// Cenário 1.2: Saldo insuficiente
Console.WriteLine("\n--- Cenário 1.2: Saldo insuficiente ---");
var failTransferInput = new TransferMoneyInput
{
    FromAccountId = joaoAccountId,
    ToAccountId = mariaAccountId,
    Amount = 100000.00m
};

var failTransferResult = transferScript.Execute(failTransferInput);
if (!failTransferResult.Success)
{
    Console.WriteLine($"\n✗ Erro esperado: {failTransferResult.Error}");
}

// ==========================================================
// EXEMPLO 2: Create Invoice Script
// ==========================================================
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("EXEMPLO 2: Create Invoice Transaction Script");
Console.WriteLine(new string('=', 60));

var invoiceScript = new CreateInvoiceScript(db);

// Cenário 2.1: Criação de invoice
Console.WriteLine("\n--- Cenário 2.1: Criar invoice com múltiplos itens ---");
var invoiceInput = new CreateInvoiceInput
{
    CustomerId = Guid.NewGuid(),
    CustomerName = "Empresa ABC Ltda",
    DueDays = 30,
    TaxRate = 0.18m,
    Items =
    [
        new InvoiceItemInput
        {
            Description = "Consultoria em TI",
            Quantity = 40,
            UnitPrice = 150.00m
        },
        new InvoiceItemInput
        {
            Description = "Licença de Software",
            Quantity = 5,
            UnitPrice = 500.00m
        },
        new InvoiceItemInput
        {
            Description = "Suporte Técnico",
            Quantity = 10,
            UnitPrice = 100.00m
        }
    ]
};

var invoiceResult = invoiceScript.Execute(invoiceInput);
if (invoiceResult.Success)
{
    Console.WriteLine($"\n✓ Sucesso!");
    Console.WriteLine($"  Invoice #: {invoiceResult.Data!.InvoiceNumber}");
    Console.WriteLine($"  SubTotal: {invoiceResult.Data.SubTotal:C}");
    Console.WriteLine($"  Impostos: {invoiceResult.Data.Tax:C}");
    Console.WriteLine($"  Total: {invoiceResult.Data.Total:C}");
    Console.WriteLine($"  Vencimento: {invoiceResult.Data.DueDate:d}");
}

// Cenário 2.2: Invoice sem itens
Console.WriteLine("\n--- Cenário 2.2: Invoice sem itens (erro) ---");
var emptyInvoiceInput = new CreateInvoiceInput
{
    CustomerId = Guid.NewGuid(),
    CustomerName = "Cliente Teste",
    Items = []
};

var emptyInvoiceResult = invoiceScript.Execute(emptyInvoiceInput);
if (!emptyInvoiceResult.Success)
{
    Console.WriteLine($"\n✗ Erro esperado: {emptyInvoiceResult.Error}");
}

// ==========================================================
// EXEMPLO 3: Process Refund Script
// ==========================================================
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("EXEMPLO 3: Process Refund Transaction Script");
Console.WriteLine(new string('=', 60));

var refundScript = new ProcessRefundScript(db);

// Cenário 3.1: Reembolso bem-sucedido
Console.WriteLine("\n--- Cenário 3.1: Reembolso de produto ---");
var refundInput = new ProcessRefundInput
{
    AccountId = joaoAccountId,
    ProductId = notebookProductId,
    Quantity = 1,
    Reason = "Produto com defeito"
};

var refundResult = refundScript.Execute(refundInput);
if (refundResult.Success)
{
    Console.WriteLine($"\n✓ Sucesso!");
    Console.WriteLine($"  Refund ID: {refundResult.Data!.RefundId}");
    Console.WriteLine($"  Valor reembolsado: {refundResult.Data.RefundAmount:C}");
    Console.WriteLine($"  Novo saldo conta: {refundResult.Data.NewAccountBalance:C}");
    Console.WriteLine($"  Novo estoque produto: {refundResult.Data.NewProductStock}");
}

// ==========================================================
// RESUMO
// ==========================================================
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("RESUMO - TRANSACTION SCRIPT PATTERN");
Console.WriteLine(new string('=', 60));

Console.WriteLine(@"
┌─────────────────────────────────────────────────────────────┐
│ CARACTERÍSTICAS DO PADRÃO                                   │
├─────────────────────────────────────────────────────────────┤
│ ✓ Lógica de negócio em procedimentos simples                │
│ ✓ Cada script = uma transação/operação                      │
│ ✓ Acesso direto ao banco via Data Gateway                   │
│ ✓ Validação e regras inline no script                       │
│ ✓ Ideal para CRUD e lógica simples                          │
├─────────────────────────────────────────────────────────────┤
│ QUANDO USAR                                                 │
├─────────────────────────────────────────────────────────────┤
│ • Aplicações com lógica de negócio simples                  │
│ • Sistemas CRUD                                             │
│ • Prototipagem rápida                                       │
│ • Scripts e utilitários                                     │
├─────────────────────────────────────────────────────────────┤
│ QUANDO EVITAR                                               │
├─────────────────────────────────────────────────────────────┤
│ • Lógica de domínio complexa                                │
│ • Sistema com muitas regras de negócio                      │
│ • Necessidade de alta testabilidade                         │
│ • Código duplicado entre scripts                            │
└─────────────────────────────────────────────────────────────┘
");

Console.WriteLine("Demonstração concluída!");
