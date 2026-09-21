using Host;
using Microsoft.Extensions.Logging;
using Modules.Abstractions.Contracts;
using Modules.Abstractions.Events;
using Modules.Catalog;
using Modules.Orders;
using Modules.Payments;

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss.fff ";
    });
});

Console.WriteLine("ModularMonolithCommerceDemo - modulos com contratos explicitos, dados isolados e eventos internos");

// O host e o UNICO lugar que conhece os tres modulos concretos. Cada modulo conhece
// apenas as abstracoes e, quando muito, o contrato publico de outro.
IEventBus bus = new InProcessEventBus(loggerFactory.CreateLogger<InProcessEventBus>());

ICatalogModule catalog = new CatalogModule(bus, loggerFactory.CreateLogger<CatalogModule>());
IPaymentsModule payments = new PaymentsModule(bus, loggerFactory.CreateLogger<PaymentsModule>());
IOrdersModule orders = new OrdersModule(catalog, bus, loggerFactory.CreateLogger<OrdersModule>());

ILogger demo = loggerFactory.CreateLogger("Demo");

Section("1. Pedido que da certo: estoque reservado e pagamento confirmado");
string ok = await orders.PlaceOrderAsync("cliente-1", "ABC-1", 2, CancellationToken.None);
await ShowAsync();

Section("2. Pedido recusado no pagamento: acima do limite");
string tooExpensive = await orders.PlaceOrderAsync("cliente-2", "XYZ-9", 2, CancellationToken.None);
await ShowAsync();

Section("3. Pedido sem estoque suficiente");
string noStock = await orders.PlaceOrderAsync("cliente-3", "XYZ-9", 50, CancellationToken.None);
await ShowAsync();

Section("4. O que cada modulo expoe pelo contrato");
foreach (ProductInfo product in await catalog.ListAsync(CancellationToken.None))
{
    // ProductInfo nao tem SupplierCode: o contrato publico nao expoe esse campo interno.
    demo.LogInformation("  catalogo: {Sku} {Nome} {Preco:F2}, estoque {Estoque}", product.Sku, product.Name, product.Price, product.AvailableStock);
}

foreach (PaymentInfo payment in await payments.ListAsync(CancellationToken.None))
{
    // PaymentInfo nao tem GatewayReference, pelo mesmo motivo.
    demo.LogInformation("  pagamento: {Pedido} {Valor:F2} {Status}", payment.OrderId, payment.Amount, payment.Status);
}

demo.LogInformation(
    "Nenhum modulo leu a tabela de outro: Pedidos consultou o Catalogo pelo contrato, e o resto foi por evento.");

await Task.Delay(200);
Console.WriteLine();
Console.WriteLine("Fim dos cenarios.");

async Task ShowAsync()
{
    foreach (OrderInfo order in await orders.ListAsync(CancellationToken.None))
    {
        demo.LogInformation("  pedido {Pedido}: {Quantidade}x {Sku}, total {Total:F2} -> {Status}",
            order.OrderId, order.Quantity, order.Sku, order.Total, order.Status);
    }
}

static void Section(string title)
{
    Thread.Sleep(120);
    Console.WriteLine();
    Console.WriteLine("=== " + title + " ===");
}
