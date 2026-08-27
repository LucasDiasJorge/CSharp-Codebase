namespace StrategyResolver.Domain;

/// <summary>
/// Métodos de pagamento aceitos pela plataforma. São constantes de dados, não um enum:
/// o valor chega de fora (checkout, fila, integração de parceiro) e o exemplo precisa
/// suportar um método desconhecido para exercitar o fallback.
/// </summary>
public static class PaymentMethods
{
    public const string Pix = "PIX";
    public const string Boleto = "BOLETO";
    public const string CreditCard = "CREDIT_CARD";
    public const string DigitalWallet = "DIGITAL_WALLET";
}
