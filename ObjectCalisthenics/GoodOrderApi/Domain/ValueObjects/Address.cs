namespace GoodOrderApi.Domain.ValueObjects;

// ✅ REGRA 8: Máximo duas variáveis de instância
// ✅ REGRA 3: Encapsular primitivos
// ✅ REGRA 6: Não usar abreviações

/// <summary>
/// Value Object que representa um endereço completo.
/// Agrupa informações relacionadas em um único objeto coeso.
/// </summary>
public sealed class Address
{
    public string Street { get; }
    public CityInfo City { get; }

    private Address(string street, CityInfo city)
    {
        Street = street;
        City = city;
    }

    public static Address Create(string street, string city, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street cannot be empty");
        
        var cityInfo = CityInfo.Create(city, zipCode);
        return new Address(street, cityInfo);
    }

    public override string ToString() => $"{Street}, {City}";
}

/// <summary>
/// Value Object que representa informações da cidade.
/// ✅ REGRA 8: Máximo duas variáveis de instância
/// </summary>
public sealed class CityInfo
{
    public string Name { get; }
    public string ZipCode { get; }

    private CityInfo(string name, string zipCode)
    {
        Name = name;
        ZipCode = zipCode;
    }

    public static CityInfo Create(string name, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("City name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new DomainException("Zip code cannot be empty");
        
        return new CityInfo(name, zipCode);
    }

    public override string ToString() => $"{Name} - {ZipCode}";
}
