using UseCases.Core;
using UseCases.Examples.ProcessOrder.DTOs;
using UseCases.Examples.ProcessOrder.Entities;
using UseCases.Examples.ProcessOrder.Interfaces;

namespace UseCases.Examples.ProcessOrder;

/// <summary>
/// Use Case: Processar Pedido
/// 
/// Demonstra um fluxo complexo de e-commerce:
/// - Validação de cliente e produtos
/// - Verificação e reserva de estoque
/// - Cálculo de descontos por tier
/// - Processamento de pagamento
/// - Notificação por email
/// </summary>
public class ProcessOrderUseCase : IUseCase<ProcessOrderInput, Result<ProcessOrderOutput>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IPaymentGateway paymentGateway,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _paymentGateway = paymentGateway;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProcessOrderOutput>> ExecuteAsync(
        ProcessOrderInput input,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar entrada
        if (!input.Items.Any())
            return Result<ProcessOrderOutput>.Failure("Pedido deve conter pelo menos um item");

        // 2. Buscar cliente
        var customer = await _customerRepository.GetByIdAsync(input.CustomerId, cancellationToken);
        if (customer is null)
            return Result<ProcessOrderOutput>.Failure("Cliente não encontrado");

        // 3. Criar pedido
        var order = Order.Create(input.CustomerId, input.ShippingAddress, input.PaymentMethod);

        // 4. Processar itens
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var itemInput in input.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemInput.ProductId, cancellationToken);
                if (product is null)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<ProcessOrderOutput>.Failure($"Produto {itemInput.ProductId} não encontrado");
                }

                // Reservar estoque
                var stockResult = product.ReserveStock(itemInput.Quantity);
                if (stockResult.IsFailure)
                {
                    await _unitOfWork.RollbackAsync(cancellationToken);
                    return Result<ProcessOrderOutput>.Failure(stockResult.Error);
                }

                await _productRepository.UpdateAsync(product, cancellationToken);
                order.AddItem(product, itemInput.Quantity);
            }

            // 5. Aplicar desconto baseado no tier do cliente
            var discount = customer.GetDiscountPercentage();
            if (discount > 0)
            {
                order.ApplyDiscount(discount);
            }

            // 6. Processar pagamento
            var paymentResult = await _paymentGateway.ProcessPaymentAsync(
                order.Id,
                order.FinalAmount,
                input.PaymentMethod,
                cancellationToken
            );

            if (!paymentResult.Success)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return Result<ProcessOrderOutput>.Failure($"Falha no pagamento: {paymentResult.ErrorMessage}");
            }

            order.ConfirmPayment();

            // 7. Persistir pedido
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // 8. Enviar confirmação por email
            _ = _emailService.SendOrderConfirmationAsync(
                customer.Email,
                order.OrderNumber,
                order.FinalAmount,
                cancellationToken
            );

            // 9. Retornar resultado
            return Result<ProcessOrderOutput>.Success(new ProcessOrderOutput(
                order.Id,
                order.OrderNumber,
                order.TotalAmount,
                order.Discount,
                order.FinalAmount,
                order.EstimatedDelivery,
                order.Status
            ));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
