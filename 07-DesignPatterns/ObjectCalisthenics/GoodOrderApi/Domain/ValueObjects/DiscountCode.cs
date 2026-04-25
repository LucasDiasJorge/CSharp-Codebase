namespace GoodOrderApi.Domain.ValueObjects;

// ✅ REGRA 3: Encapsular primitivos - Código de desconto como Value Object
// ✅ REGRA 2: Evitar ELSE - usando pattern matching

/// <summary>
/// Value Object que representa um código de desconto.
/// Encapsula a lógica de aplicação do desconto.
/// </summary>
public abstract class DiscountCode
{
    public abstract string Code { get; }
    public abstract decimal Percentage { get; }

    public Money ApplyTo(Money originalAmount)
    {
        return originalAmount.ApplyDiscount(Percentage);
    }

    public Money CalculateDiscountAmount(Money originalAmount)
    {
        return Money.Create(originalAmount.Amount * (Percentage / 100));
    }

    public static DiscountCode? TryCreate(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return code.ToUpperInvariant() switch
        {
            "SAVE10" => new TenPercentDiscount(),
            "SAVE20" => new TwentyPercentDiscount(),
            "SAVE50" => new FiftyPercentDiscount(),
            _ => null
        };
    }

    public override string ToString() => Code;
}

public sealed class TenPercentDiscount : DiscountCode
{
    public override string Code => "SAVE10";
    public override decimal Percentage => 10;
}

public sealed class TwentyPercentDiscount : DiscountCode
{
    public override string Code => "SAVE20";
    public override decimal Percentage => 20;
}

public sealed class FiftyPercentDiscount : DiscountCode
{
    public override string Code => "SAVE50";
    public override decimal Percentage => 50;
}
