namespace EventSourcingBankAccountDemo.Events;

/// <summary>
/// Um fato que já aconteceu. Eventos são imutáveis e nomeados no passado: não se
/// cancela nem se edita um evento, porque o passado não muda. Corrigir significa
/// acrescentar um evento novo que compense o anterior.
/// </summary>
public abstract record AccountEvent
{
    /// <summary>Posição deste evento no fluxo do agregado. Começa em 1.</summary>
    public long Version { get; init; }

    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Descrição legível, usada só para exibir o fluxo.</summary>
    public abstract string Describe();
}

public sealed record AccountOpened(string AccountId, string Owner, decimal InitialDeposit) : AccountEvent
{
    public override string Describe() => $"conta aberta para {Owner} com {InitialDeposit:F2}";
}

public sealed record MoneyDeposited(decimal Amount) : AccountEvent
{
    public override string Describe() => $"deposito de {Amount:F2}";
}

public sealed record MoneyWithdrawn(decimal Amount) : AccountEvent
{
    public override string Describe() => $"saque de {Amount:F2}";
}

public sealed record AccountFrozen(string Reason) : AccountEvent
{
    public override string Describe() => $"conta congelada: {Reason}";
}

public sealed record AccountUnfrozen() : AccountEvent
{
    public override string Describe() => "conta descongelada";
}
