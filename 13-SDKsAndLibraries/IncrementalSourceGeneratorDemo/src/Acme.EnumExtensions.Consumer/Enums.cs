using Acme.EnumExtensions;

/// <summary>Enum no namespace global, para o gerador tratar esse caso tambem.</summary>
[EnumExtensions]
public enum OrderStatus
{
    Draft,
    AwaitingPayment,
    Paid,
    Shipped,
    Delivered,
    Cancelled,
}

[EnumExtensions]
internal enum Priority
{
    Low,
    Medium,
    High,
}

/// <summary>Enum SEM o atributo: o gerador ignora, e nenhum arquivo e produzido.</summary>
public enum NotGenerated
{
    A,
    B,
}

namespace Shipping
{
    [EnumExtensions]
    public enum Carrier
    {
        Correios,
        Jadlog,
        Loggi,
    }
}
