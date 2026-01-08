namespace GoodOrderApi.Domain.ValueObjects;

// ✅ REGRA 3: Encapsular primitivos e strings
// ✅ REGRA 6: Não usar abreviações
// ✅ REGRA 9: Evitar getters/setters - expor comportamento

/// <summary>
/// Value Object que representa um endereço de email válido.
/// Encapsula a validação e o valor.
/// </summary>
public sealed class Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        Validate(value);
        return new Email(value);
    }

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");

        if (!value.Contains('@'))
            throw new DomainException("Email must contain @");
    }

    public override string ToString() => Value;
    
    public override bool Equals(object? obj) => 
        obj is Email other && Value == other.Value;
    
    public override int GetHashCode() => Value.GetHashCode();
}

/// <summary>
/// Value Object que representa um número de telefone.
/// </summary>
public sealed class PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty");
        
        return new PhoneNumber(value);
    }

    public override string ToString() => Value;
}

/// <summary>
/// Value Object que representa um nome de pessoa.
/// </summary>
public sealed class PersonName
{
    public string Value { get; }

    private PersonName(string value)
    {
        Value = value;
    }

    public static PersonName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Name cannot be empty");
        
        if (value.Length < 2)
            throw new DomainException("Name must have at least 2 characters");
        
        return new PersonName(value);
    }

    public override string ToString() => Value;
}

/// <summary>
/// Value Object que representa uma quantidade de itens.
/// Garante que a quantidade seja sempre positiva.
/// </summary>
public sealed class Quantity
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity Create(int value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be greater than zero");
        
        return new Quantity(value);
    }

    public Quantity Add(Quantity other) => new(Value + other.Value);
    
    public Quantity Subtract(Quantity other)
    {
        var result = Value - other.Value;
        if (result < 0)
            throw new DomainException("Cannot have negative quantity");
        return new Quantity(result);
    }

    public bool IsGreaterThanOrEqual(Quantity other) => Value >= other.Value;

    public override string ToString() => Value.ToString();
}

/// <summary>
/// Value Object que representa um valor monetário.
/// Encapsula operações financeiras.
/// </summary>
public sealed class Money
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    public static Money Zero => new(0);
    
    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Money cannot be negative");
        
        return new Money(Math.Round(amount, 2));
    }

    public Money Add(Money other) => new(Amount + other.Amount);
    
    public Money Subtract(Money other)
    {
        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainException("Insufficient funds");
        return new Money(result);
    }

    public Money MultiplyBy(int multiplier) => new(Amount * multiplier);
    
    public Money MultiplyBy(decimal multiplier) => new(Math.Round(Amount * multiplier, 2));

    public Money ApplyDiscount(decimal percentage)
    {
        var discount = Amount * (percentage / 100);
        return new Money(Amount - discount);
    }

    public override string ToString() => $"${Amount:F2}";
    
    public override bool Equals(object? obj) => 
        obj is Money other && Amount == other.Amount;
    
    public override int GetHashCode() => Amount.GetHashCode();
}

/// <summary>
/// Exceção de domínio para regras de negócio violadas.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
