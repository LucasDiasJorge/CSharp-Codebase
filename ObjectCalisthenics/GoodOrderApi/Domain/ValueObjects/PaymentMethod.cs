namespace GoodOrderApi.Domain.ValueObjects;

// ✅ REGRA 3: Encapsular primitivos - Método de pagamento como Value Object
// ✅ REGRA 2: Evitar ELSE - usando pattern matching

/// <summary>
/// Value Object que representa um método de pagamento.
/// </summary>
public abstract class PaymentMethod
{
    public abstract string Name { get; }
    public abstract string DisplayName { get; }

    public static PaymentMethod Create(string method)
    {
        return method switch
        {
            "CreditCard" => new CreditCardPayment(),
            "DebitCard" => new DebitCardPayment(),
            "Pix" => new PixPayment(),
            "BankTransfer" => new BankTransferPayment(),
            _ => throw new DomainException($"Invalid payment method: {method}")
        };
    }

    public override string ToString() => Name;
}

public sealed class CreditCardPayment : PaymentMethod
{
    public override string Name => "CreditCard";
    public override string DisplayName => "Credit Card";
}

public sealed class DebitCardPayment : PaymentMethod
{
    public override string Name => "DebitCard";
    public override string DisplayName => "Debit Card";
}

public sealed class PixPayment : PaymentMethod
{
    public override string Name => "Pix";
    public override string DisplayName => "Pix";
}

public sealed class BankTransferPayment : PaymentMethod
{
    public override string Name => "BankTransfer";
    public override string DisplayName => "Bank Transfer";
}
