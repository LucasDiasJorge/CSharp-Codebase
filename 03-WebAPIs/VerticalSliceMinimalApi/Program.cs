using VerticalSliceMinimalApi.Features.Orders.Create;
using VerticalSliceMinimalApi.Features.Orders.GetById;
using VerticalSliceMinimalApi.Features.Orders.List;
using VerticalSliceMinimalApi.Infrastructure.Orders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetOrderByIdHandler>();
builder.Services.AddScoped<ListOrdersHandler>();

WebApplication app = builder.Build();

RouteGroupBuilder ordersGroup = app.MapGroup("/orders").WithTags("Orders");

ordersGroup.MapCreateOrder();
ordersGroup.MapGetOrderById();
ordersGroup.MapListOrders();

app.MapGet("/", () => Results.Redirect("/orders")).ExcludeFromDescription();

app.Run();
