namespace TestDoublesDemo.Domain;

public enum CheckoutOutcome
{
    Confirmed,
    EmptyOrder,
    OutOfStock,
}

/// <summary>
/// Resultado do checkout como valor. É o que a <b>verificação de estado</b> observa:
/// o que saiu, sem depender de como foi feito.
/// </summary>
public sealed record CheckoutResult(CheckoutOutcome Outcome, string? Reason = null)
{
    public bool IsConfirmed => Outcome == CheckoutOutcome.Confirmed;

    public static CheckoutResult Confirmed() => new CheckoutResult(CheckoutOutcome.Confirmed);

    public static CheckoutResult Rejected(CheckoutOutcome outcome, string reason) =>
        new CheckoutResult(outcome, reason);
}
