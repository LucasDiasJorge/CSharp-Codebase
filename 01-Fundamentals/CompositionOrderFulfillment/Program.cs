using CompositionOrderFulfillment.Models;
using CompositionOrderFulfillment.Services;

Console.WriteLine("Composition - Pedido de E-commerce");
Console.WriteLine(new string('=', 44));

OrderApplicationService orderApplicationService = new OrderApplicationService();

Console.WriteLine("Catalogo disponivel:");

foreach (Product product in orderApplicationService.GetCatalog())
{
    Console.WriteLine($"- {product.Sku} | {product.Description} | {product.UnitPrice:C}");
}

PurchaseOrder purchaseOrder = orderApplicationService.StartOrder("Mercado Central Ltda");

Product keyboard = orderApplicationService.GetProductBySku("KB-100");
Product headset = orderApplicationService.GetProductBySku("HS-300");

PurchaseOrder.OrderLine keyboardLine = purchaseOrder.AddItem(keyboard, 2);
purchaseOrder.AddItem(headset, 1);

PrintOrder(purchaseOrder);

purchaseOrder.Confirm();
Console.WriteLine();
Console.WriteLine($"Pedido confirmado com status: {purchaseOrder.Status}");

Console.WriteLine($"Linha {keyboardLine.ProductSku} ativa antes do cancelamento: {keyboardLine.IsActive}");

purchaseOrder.Cancel();

Console.WriteLine($"Linha {keyboardLine.ProductSku} ativa depois do cancelamento: {keyboardLine.IsActive}");
Console.WriteLine($"Status final do pedido: {purchaseOrder.Status}");
Console.WriteLine($"Quantidade de linhas ativas no pedido: {purchaseOrder.OrderLines.Count}");

Console.WriteLine();
Console.WriteLine("Ownership forte (Composition):");
Console.WriteLine("As linhas pertencem ao pedido e sao invalidadas quando o pedido e cancelado.");

static void PrintOrder(PurchaseOrder purchaseOrder)
{
    Console.WriteLine();
    Console.WriteLine($"Pedido {purchaseOrder.Id}");
    Console.WriteLine($"Cliente: {purchaseOrder.CustomerName}");
    Console.WriteLine($"Status: {purchaseOrder.Status}");

    foreach (PurchaseOrder.OrderLine orderLine in purchaseOrder.OrderLines)
    {
        Console.WriteLine($"- {orderLine}");
    }

    Console.WriteLine($"Total: {purchaseOrder.GetTotalAmount():C}");
}